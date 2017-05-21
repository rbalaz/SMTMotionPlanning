using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Z3;
using System.Globalization;
using System.IO;

namespace SMTMotionPlanning
{
    class PathFinding
    {
        public class TestFailedException : Exception { }
        class IllegalConversionException : Exception { }

        private Coordinate startLocation;
        private Coordinate goalLocation;
        private int pathSegments;
        private Space world;
        private int obstaclePassDistance;
        private int maximumPathLength;
        private bool curvedPath;

        public PathFinding(Coordinate startLocation, Coordinate goalLocation, int pathSegments,
            int obstaclePassDistance, int maximumPathLength, Space world, bool curvedPath)
        {
            this.startLocation = startLocation;
            this.goalLocation = goalLocation;
            this.pathSegments = pathSegments;
            this.obstaclePassDistance = obstaclePassDistance;
            this.world = world;
            this.maximumPathLength = maximumPathLength;
            this.curvedPath = curvedPath;
        }

        public Coordinate[] findPath()
        {
            Coordinate[] path = new Coordinate[pathSegments + 1];

            Context ctx = new Context();

            IntExpr[] sourcesX = new IntExpr[pathSegments];
            IntExpr[] sourcesY = new IntExpr[pathSegments];
            for (int i = 0; i < pathSegments; i++)
            {
                sourcesX[i] = ctx.MkIntConst("xs" + i);
                sourcesY[i] = ctx.MkIntConst("ys" + i);
            }

            IntExpr[] destinationsX = new IntExpr[pathSegments];
            IntExpr[] destinationsY = new IntExpr[pathSegments];
            for (int i = 0; i < pathSegments; i++)
            {
                destinationsX[i] = ctx.MkIntConst("xd" + i);
                destinationsY[i] = ctx.MkIntConst("yd" + i);
            }

            BoolExpr worldSizeConstraints = generateWorldSizeConstraints(ctx, sourcesX, sourcesY, destinationsX, destinationsY);
            BoolExpr movementConstraints = generateMovementConstraints(ctx, sourcesX, sourcesY, destinationsX, destinationsY);
            BoolExpr orthogonalConstraints = generateOrthogonalConstraints(ctx, sourcesX, sourcesY, destinationsX, destinationsY);
            BoolExpr prerequisitesConstraints = generateStartAndGoal(ctx, sourcesX, sourcesY, destinationsX, destinationsY);
            BoolExpr joiningPathSegments = joinPathSegments(ctx, sourcesX, sourcesY, destinationsX, destinationsY);
            BoolExpr avoidingObstacles = avoidObstacles(world, ctx, sourcesX, sourcesY, destinationsX, destinationsY);
            BoolExpr pathLengthConstraint = generatePathLengthConstraint(ctx, sourcesX, sourcesY, destinationsX, destinationsY);

            Solver s = ctx.MkSolver();
            s.Assert(worldSizeConstraints);
            if (!(curvedPath))
            {
                s.Assert(movementConstraints);
                s.Assert(orthogonalConstraints);
            }
            s.Assert(prerequisitesConstraints);
            s.Assert(joiningPathSegments);
            s.Assert(avoidingObstacles);
            s.Assert(pathLengthConstraint);

            Status status = s.Check();
            if (status != Status.SATISFIABLE)
                throw new TestFailedException();

            for (int i = 0; i < pathSegments; i++)
            {
                path[i] = new Coordinate(convertExprToInt(s.Model.ConstInterp(sourcesX[i])),
                    convertExprToInt(s.Model.ConstInterp(sourcesY[i])));
            }
            path[pathSegments] = new Coordinate(convertExprToInt(s.Model.ConstInterp(destinationsX[pathSegments - 1])),
                convertExprToInt(s.Model.ConstInterp(destinationsY[pathSegments - 1])));

            FileStream stream = new FileStream("solverinputs.txt", FileMode.Create, FileAccess.Write);
            StreamWriter writer = new StreamWriter(stream);
            writer.WriteLine(worldSizeConstraints.ToString());
            writer.WriteLine(movementConstraints.ToString());
            writer.WriteLine(orthogonalConstraints.ToString());
            writer.WriteLine(prerequisitesConstraints.ToString());
            writer.WriteLine(joiningPathSegments.ToString());
            writer.WriteLine(avoidingObstacles.ToString());
            writer.WriteLine(pathLengthConstraint.ToString());
            writer.Close();
            stream.Close();

            return path;
        }

        private int convertExprToInt(Expr expr)
        {
            if (expr.IsInt)
            {
                string s = expr.ToString();
                int output;
                int.TryParse(s, out output);
                return output;
            }
            else if (expr.IsReal)
            {
                string s = expr.ToString();
                double output;
                double.TryParse(s, out output);
                return (int)output;
            }
            else
                throw new IllegalConversionException();
        }

        private BoolExpr generateWorldSizeConstraints(Context ctx, IntExpr[] sourcesX, IntExpr[] sourcesY, 
            IntExpr[] destinationsX, IntExpr[] destinationsY)
        {
            // First input into SMT solver: Field of constrainsConj.
            // Contains constrains for movement so that agent will not try to move out of world, surpassing one its parameters
            BoolExpr[] constrains = new BoolExpr[8 * pathSegments];
            IntExpr worldLength = ctx.MkInt(world.length);
            IntExpr worldWidth = ctx.MkInt(world.width);
            for (int i = 0; i < constrains.Length; i = i + 8)
            {
                constrains[i] = ctx.MkLe(ctx.MkInt("0"), sourcesX[i / 8]);
                constrains[i + 1] = ctx.MkLe(sourcesX[i / 8], worldWidth);
                constrains[i + 2] = ctx.MkLe(ctx.MkInt("0"), sourcesY[i / 8]);
                constrains[i + 3] = ctx.MkLe(sourcesY[i / 8], worldLength);
                constrains[i + 4] = ctx.MkLe(ctx.MkInt("0"), destinationsX[i / 8]);
                constrains[i + 5] = ctx.MkLe(destinationsX[i / 8], worldWidth);
                constrains[i + 6] = ctx.MkLe(ctx.MkInt("0"), destinationsY[i / 8]);
                constrains[i + 7] = ctx.MkLe(destinationsY[i / 8], worldLength);
            }

            return ctx.MkAnd(constrains);
        }

        private BoolExpr generateMovementConstraints(Context ctx, IntExpr[] sourcesX, IntExpr[] sourcesY, 
            IntExpr[] destinationsX, IntExpr[] destinationsY)
        {
            // Second input into SMT solver: field of bothDogsConj
            BoolExpr[] dogX = new BoolExpr[pathSegments];
            BoolExpr[] dogY = new BoolExpr[pathSegments];
            for (int i = 0; i < pathSegments; i++)
            {
                dogX[i] = ctx.MkEq(sourcesX[i], destinationsX[i]);
                dogY[i] = ctx.MkEq(sourcesY[i], destinationsY[i]);
            }
            BoolExpr[] bothDogs = new BoolExpr[pathSegments];
            for (int i = 0; i < pathSegments; i++)
            {
                bothDogs[i] = ctx.MkXor(dogX[i], dogY[i]);
            }

            return ctx.MkAnd(bothDogs);
        }

        private BoolExpr generateOrthogonalConstraints(Context ctx, IntExpr[] sourcesX, IntExpr[] sourcesY,
            IntExpr[] destinationsX, IntExpr[] destinationsY)
        {
            BoolExpr[] orthogonalConstraintX = new BoolExpr[pathSegments - 1];
            BoolExpr[] orthogonalConstraintY = new BoolExpr[pathSegments - 1];
            for (int i = 0; i < pathSegments - 1; i++)
            {
                orthogonalConstraintX[i] = ctx.MkOr(ctx.MkEq(sourcesX[i], sourcesX[i + 1]),
                    ctx.MkEq(destinationsX[i], destinationsX[i + 1]));
                orthogonalConstraintY[i] = ctx.MkOr(ctx.MkEq(sourcesY[i], sourcesY[i + 1]),
                    ctx.MkEq(destinationsY[i], destinationsY[i + 1]));
            }

            return ctx.MkAnd(ctx.MkAnd(orthogonalConstraintX), ctx.MkAnd(orthogonalConstraintY));
        }

        private BoolExpr generateStartAndGoal(Context ctx, IntExpr[] sourcesX, IntExpr[] sourcesY, IntExpr[] destinationsX, 
            IntExpr[] destinationsY)
        {
            // Third input into SMT solver: prerequisites expression
            // These expressions only mark the start and goal location in the sources and destinations fields
            IntExpr startX = ctx.MkInt(startLocation.x);
            IntExpr startY = ctx.MkInt(startLocation.y);
            IntExpr goalX = ctx.MkInt(goalLocation.x);
            IntExpr goalY = ctx.MkInt(goalLocation.y);

            BoolExpr initialXPosition = ctx.MkEq(sourcesX[0], startX);
            BoolExpr initialYPosition = ctx.MkEq(sourcesY[0], startY);
            BoolExpr goalXPosition = ctx.MkEq(destinationsX[pathSegments - 1], goalX);
            BoolExpr goalYPosition = ctx.MkEq(destinationsY[pathSegments - 1], goalY);

            return ctx.MkAnd(new BoolExpr[] { initialXPosition, initialYPosition, goalXPosition, goalYPosition });
        }

        private BoolExpr joinPathSegments(Context ctx, IntExpr[] sourcesX, IntExpr[] sourcesY, IntExpr[] destinationsX, 
            IntExpr[] destinationsY)
        {
            // Fourth input into SMT solver: pathSegmentsConj field
            // Connection of previous and following path segments
            BoolExpr[] pathSegmentsX = new BoolExpr[pathSegments - 1];
            BoolExpr[] pathSegmentsY = new BoolExpr[pathSegments - 1];
            for (int i = 0; i < pathSegmentsX.Length; i++)
            {
                pathSegmentsX[i] = ctx.MkEq(sourcesX[i + 1], destinationsX[i]);
                pathSegmentsY[i] = ctx.MkEq(sourcesY[i + 1], destinationsY[i]);
            }
            BoolExpr pathSegmentsXConj = ctx.MkAnd(pathSegmentsX);
            BoolExpr pathSegmentsYConj = ctx.MkAnd(pathSegmentsY);

            return ctx.MkAnd(pathSegmentsXConj, pathSegmentsYConj);
        }

        private BoolExpr generatePathLengthConstraint(Context ctx, IntExpr[] sourcesX, IntExpr[] sourcesY, 
            IntExpr[] destinationsX, IntExpr[] destinationsY)
        {
            // Potentially sixth input into SMT solver is the path length constraint
            ArithExpr[] lengthParts = new ArithExpr[pathSegments];
            for (int i = 0; i < pathSegments; i++)
            {
                ArithExpr xs_minus_xd = ctx.MkSub(sourcesX[i], destinationsX[i]);
                ArithExpr xd_minus_xs = ctx.MkSub(destinationsX[i], sourcesX[i]);
                ArithExpr ys_minus_yd = ctx.MkSub(sourcesY[i], destinationsY[i]);
                ArithExpr yd_minus_ys = ctx.MkSub(destinationsY[i], sourcesY[i]);
                BoolExpr xs_ge_xd = ctx.MkGe(sourcesX[i], destinationsX[i]);
                BoolExpr ys_ge_yd = ctx.MkGe(sourcesY[i], destinationsY[i]);
                ArithExpr first = (ArithExpr)ctx.MkITE(xs_ge_xd, xs_minus_xd, xd_minus_xs);
                ArithExpr second = (ArithExpr)ctx.MkITE(ys_ge_yd, ys_minus_yd, yd_minus_ys);
                lengthParts[i] = ctx.MkAdd(first, second);
            }
            BoolExpr finalExpression = ctx.MkLe(ctx.MkAdd(lengthParts), ctx.MkInt(maximumPathLength));

            return finalExpression;
        }

        private BoolExpr avoidObstacles(Space world, Context ctx, IntExpr[] sourcesX, IntExpr[] sourcesY, 
            IntExpr[] destinationsX, IntExpr[] destinationsY)
        {
            // Fifth input into SMT solver: avoidingObstaclesCombinedConj field
            // Defines the way on how agent is supposed to avoid obstacles, including distance it needs to keep when 
            // moving around them
            List<BoolExpr> obstacles = new List<BoolExpr>();
            for (int i = 0; i < world.obstacles.Count; i++)
            {
                switch (world.obstacles[i].type)
                {
                    case Obstacle.ObstacleType.Polygon:
                        obstacles.Add(handlePolygonalObstacle((PolygonalObstacle)world.obstacles[i], ctx, sourcesX, sourcesY,
                            destinationsX, destinationsY));
                        obstacles.Add(handlePolygonEndPoints((PolygonalObstacle)world.obstacles[i], ctx, sourcesX, sourcesY,
                            destinationsX, destinationsY));
                        break;
                    case Obstacle.ObstacleType.Ellipse:
                        obstacles.Add(handleEllipticalObstacle((EllipticalObstacle)world.obstacles[i], ctx, destinationsX, destinationsY, sourcesX, sourcesY));
                        break;
                    case Obstacle.ObstacleType.Rectangle:
                        if (((RectangularObstacle)(world.obstacles[i])).realLocation != null)
                            obstacles.Add(handleRealRectangularObstacle((RectangularObstacle)world.obstacles[i], ctx, sourcesX, sourcesY, destinationsX, destinationsY));
                        else
                            obstacles.Add(handleRectangularObstacle((RectangularObstacle)world.obstacles[i], ctx, sourcesX, sourcesY, destinationsX, destinationsY));
                        break;
                }
            }

            return ctx.MkAnd(obstacles.ToArray());
        }

        private BoolExpr handleEllipticalObstacle(EllipticalObstacle obstacle, Context ctx, IntExpr[] destinationsX,
            IntExpr[] destinationsY, IntExpr[] sourcesX, IntExpr[] sourcesY)
        {
            if (obstacle.length != obstacle.width)
            {
                RectangularObstacle rectangle = new RectangularObstacle(obstacle.length, obstacle.width,
                    new Coordinate(obstacle.location.x - obstacle.width / 2, obstacle.location.y - obstacle.length / 2));
                return handleRectangularObstacle(rectangle, ctx, sourcesX, sourcesY, destinationsX, destinationsY);
            }
            else
            {
                BoolExpr[] avoidingCircle = new BoolExpr[pathSegments];
                NumberFormatInfo dot = new NumberFormatInfo();
                dot.NumberDecimalSeparator = ".";
                for (int i = 0; i < pathSegments; i++)
                {
                    double radius = obstacle.length / 2.0 + obstaclePassDistance;
                    int translateX = obstacle.location.x;
                    int translateY = obstacle.location.y;
                    IntExpr dist = ctx.MkInt((int)radius);
                    ArithExpr adjustedSourceX = ctx.MkSub(sourcesX[i], ctx.MkInt(translateX));
                    ArithExpr adjustedSourceY = ctx.MkSub(sourcesY[i], ctx.MkInt(translateY));
                    ArithExpr adjustedDestX = ctx.MkSub(destinationsX[i], ctx.MkInt(translateX));
                    ArithExpr adjustedDestY = ctx.MkSub(destinationsY[i], ctx.MkInt(translateY));

                    ArithExpr dx = ctx.MkSub(adjustedDestX, adjustedSourceX);
                    ArithExpr dy = ctx.MkSub(adjustedDestY, adjustedSourceY);
                    ArithExpr dr_2 = ctx.MkAdd(ctx.MkMul(dx, dx), ctx.MkMul(dy, dy));
                    ArithExpr D = ctx.MkSub(ctx.MkMul(adjustedSourceX, adjustedDestY), ctx.MkMul(adjustedDestX, adjustedSourceY));

                    ArithExpr incidence = ctx.MkSub(ctx.MkMul(ctx.MkMul(dist, dist), dr_2), ctx.MkMul(D, D));
                    BoolExpr final = ctx.MkLe(incidence, ctx.MkInt(0));
                    avoidingCircle[i] = final;
                }
                return ctx.MkAnd(avoidingCircle);
            }
        }

        private BoolExpr handleRectangularObstacle(RectangularObstacle obstacle, Context ctx, IntExpr[] sourcesX, 
            IntExpr[] sourcesY, IntExpr[] destinationsX, IntExpr[] destinationsY)
        {
            IntExpr obstacleLeftX = ctx.MkInt(obstacle.location.x);
            IntExpr obstacleRightX = ctx.MkInt(obstacle.location.x + obstacle.width);
            IntExpr obstacleLeftY = ctx.MkInt(obstacle.location.y);
            IntExpr obstacleRightY = ctx.MkInt(obstacle.location.y + obstacle.length);
            IntExpr passDistance = ctx.MkInt(obstaclePassDistance);

            BoolExpr[] avoidingObstaclesX = new BoolExpr[pathSegments];
            BoolExpr[] avoidingObstaclesY = new BoolExpr[pathSegments];
            BoolExpr[] avoidingObstaclesCombined = new BoolExpr[pathSegments];
            for (int i = 0; i < pathSegments; i++)
            {
                ArithExpr xl_minus_xs = ctx.MkSub(obstacleLeftX, sourcesX[i]);
                ArithExpr xl_minus_xd = ctx.MkSub(obstacleLeftX, destinationsX[i]);
                ArithExpr xs_minus_xh = ctx.MkSub(sourcesX[i], obstacleRightX);
                ArithExpr xd_minus_xh = ctx.MkSub(destinationsX[i], obstacleRightX);
                avoidingObstaclesX[i] = ctx.MkOr(ctx.MkAnd(ctx.MkGe(xl_minus_xs, passDistance), ctx.MkGe(xl_minus_xd, passDistance)),
                    ctx.MkAnd(ctx.MkGe(xs_minus_xh, passDistance), ctx.MkGe(xd_minus_xh, passDistance)));

                ArithExpr yl_minus_ys = ctx.MkSub(obstacleLeftY, sourcesY[i]);
                ArithExpr yl_minus_yd = ctx.MkSub(obstacleLeftY, destinationsY[i]);
                ArithExpr ys_minus_yh = ctx.MkSub(sourcesY[i], obstacleRightY);
                ArithExpr yd_minus_yh = ctx.MkSub(destinationsY[i], obstacleRightY);
                avoidingObstaclesY[i] = ctx.MkOr(ctx.MkAnd(ctx.MkGe(yl_minus_ys, passDistance), ctx.MkGe(yl_minus_yd, passDistance)),
                    ctx.MkAnd(ctx.MkGe(ys_minus_yh, passDistance), ctx.MkGe(yd_minus_yh, passDistance)));
                avoidingObstaclesCombined[i] = ctx.MkOr(avoidingObstaclesX[i], avoidingObstaclesY[i]);
            }

            return ctx.MkAnd(avoidingObstaclesCombined);
        }

        private BoolExpr handleRealRectangularObstacle(RectangularObstacle obstacle, Context ctx, IntExpr[] sourcesX, 
            IntExpr[] sourcesY, IntExpr[] destinationsX, IntExpr[] destinationsY)
        {
            RealExpr obstacleLeftX = ctx.MkReal(obstacle.realLocation.x.ToString(new CultureInfo("en-US")));
            RealExpr obstacleRightX = ctx.MkReal((obstacle.realLocation.x + obstacle.width).ToString(new CultureInfo("en-US")));
            RealExpr obstacleLeftY = ctx.MkReal(obstacle.realLocation.y.ToString(new CultureInfo("en-US")));
            RealExpr obstacleRightY = ctx.MkReal((obstacle.realLocation.y + obstacle.length).ToString(new CultureInfo("en-US")));
            RealExpr passDistance = ctx.MkReal(obstaclePassDistance);

            BoolExpr[] avoidingObstaclesX = new BoolExpr[pathSegments];
            BoolExpr[] avoidingObstaclesY = new BoolExpr[pathSegments];
            BoolExpr[] avoidingObstaclesCombined = new BoolExpr[pathSegments];
            for (int i = 0; i < pathSegments; i++)
            {
                ArithExpr xl_minus_xs = ctx.MkSub(obstacleLeftX, sourcesX[i]);
                ArithExpr xl_minus_xd = ctx.MkSub(obstacleLeftX, destinationsX[i]);
                ArithExpr xs_minus_xh = ctx.MkSub(sourcesX[i], obstacleRightX);
                ArithExpr xd_minus_xh = ctx.MkSub(destinationsX[i], obstacleRightX);
                avoidingObstaclesX[i] = ctx.MkOr(ctx.MkAnd(ctx.MkGe(xl_minus_xs, passDistance), ctx.MkGe(xl_minus_xd, passDistance)),
                    ctx.MkAnd(ctx.MkGe(xs_minus_xh, passDistance), ctx.MkGe(xd_minus_xh, passDistance)));

                ArithExpr yl_minus_ys = ctx.MkSub(obstacleLeftY, sourcesY[i]);
                ArithExpr yl_minus_yd = ctx.MkSub(obstacleLeftY, destinationsY[i]);
                ArithExpr ys_minus_yh = ctx.MkSub(sourcesY[i], obstacleRightY);
                ArithExpr yd_minus_yh = ctx.MkSub(destinationsY[i], obstacleRightY);
                avoidingObstaclesY[i] = ctx.MkOr(ctx.MkAnd(ctx.MkGe(yl_minus_ys, passDistance), ctx.MkGe(yl_minus_yd, passDistance)),
                    ctx.MkAnd(ctx.MkGe(ys_minus_yh, passDistance), ctx.MkGe(yd_minus_yh, passDistance)));
                avoidingObstaclesCombined[i] = ctx.MkOr(avoidingObstaclesX[i], avoidingObstaclesY[i]);
            }

            return ctx.MkAnd(avoidingObstaclesCombined);
        }

        private BoolExpr handlePolygonalObstacle(PolygonalObstacle obstacle, Context ctx, IntExpr[] sourcesX, IntExpr[] sourcesY,
            IntExpr[] destinationsX, IntExpr[] destinationsY)
        {
            List<LineSegment> segments = obstacle.getLines();
            List<BoolExpr> avoidingLines = new List<BoolExpr>();
            foreach (LineSegment segment in segments)
            {
                if (segment.isHorizontal)
                {
                    int leftX = (segment.start.x < segment.end.x ? segment.start.x : segment.end.x) - obstaclePassDistance;
                    int leftY = segment.start.y - obstaclePassDistance;
                    RectangularObstacle rectangle = new RectangularObstacle(2 * obstaclePassDistance,
                        Coordinate.getXDistanceBetweenCoordinates(segment.start, segment.end) + 2 * obstaclePassDistance,
                        new Coordinate(leftX, leftY));
                    avoidingLines.Add(handleRectangularObstacle(rectangle, ctx, sourcesX, sourcesY, destinationsX, destinationsY));
                }
                else if (segment.isVertical)
                {
                    int leftY = (segment.start.y < segment.end.y ? segment.start.y : segment.end.y) - obstaclePassDistance;
                    int leftX = segment.start.x - obstaclePassDistance;
                    RectangularObstacle rectangle = new RectangularObstacle(Coordinate.getYDistanceBetweenCoordinates(segment.start, segment.end)
                        + 2 * obstaclePassDistance, 2 * obstaclePassDistance, new Coordinate(leftX, leftY));
                    avoidingLines.Add(handleRectangularObstacle(rectangle, ctx, sourcesX, sourcesY, destinationsX, destinationsY));
                }
                else
                    avoidingLines.Add(handleCurvedLine(segment, ctx, sourcesX, sourcesY, destinationsX, destinationsY));
            }

            return ctx.MkAnd(ctx.MkAnd(avoidingLines.ToArray()));
        }

        private BoolExpr handlePolygonEndPoints(PolygonalObstacle obstacle, Context ctx, IntExpr[] sourcesX, IntExpr[] sourcesY,
            IntExpr[] destinationsX, IntExpr[] destinationsY)
        {
            List<BoolExpr> avoidingEndPoints = new List<BoolExpr>();
            List<LineSegment> segments = obstacle.getLines();
            foreach (LineSegment segment in segments)
            {
                RectangularObstacle first = new RectangularObstacle(obstaclePassDistance * 2, obstaclePassDistance * 2,
                    new Coordinate(segment.start.x - obstaclePassDistance, segment.start.y - obstaclePassDistance));
                RectangularObstacle second = new RectangularObstacle(obstaclePassDistance * 2, obstaclePassDistance * 2,
                    new Coordinate(segment.end.x - obstaclePassDistance, segment.end.y - obstaclePassDistance));
                avoidingEndPoints.Add(handleRectangularObstacle(first, ctx, destinationsX, destinationsY, sourcesX, sourcesY));
                avoidingEndPoints.Add(handleRectangularObstacle(second, ctx, destinationsX, destinationsY, sourcesX, sourcesY));
            }

            return ctx.MkAnd(avoidingEndPoints.ToArray());
        }

        private BoolExpr handleCurvedLine(LineSegment segment, Context ctx, IntExpr[] sourcesX, IntExpr[] sourcesY,
            IntExpr[] destinationsX, IntExpr[] destinationsY)
        {
            double k = (double)(segment.end.y - segment.start.y) / (double)(segment.end.x - segment.start.x);
            double q = segment.start.y - k * segment.start.x;
            // Part 1: Getting the perpendicular lines equations parameters
            double kp = -1 / k;
            double qp1 = segment.start.y - kp * segment.start.x;
            double qp2 = segment.end.y - kp * segment.end.x;
            // Part 2: Getting the final line points
            double A = 1 + kp * kp;
            double B1 = 2 * kp * qp1 - 2 * segment.start.x - 2 * kp * segment.start.y;
            double B2 = 2 * kp * qp2 - 2 * segment.end.x - 2 * kp * segment.end.y;
            double C1 = segment.start.x * segment.start.x + qp1 * qp1 - 2 * qp1 * segment.start.y + 
                segment.start.y * segment.start.y - obstaclePassDistance * obstaclePassDistance;
            double C2 = segment.end.x * segment.end.x + qp2 * qp2 - 2 * qp2 * segment.end.y + 
                segment.end.y * segment.end.y - obstaclePassDistance * obstaclePassDistance;
            double x1 = (-B1 + Math.Sqrt(B1 * B1 - 4 * A * C1)) / (2 * A);
            double x2 = (-B1 - Math.Sqrt(B1 * B1 - 4 * A * C1)) / (2 * A);
            double x3 = (-B2 + Math.Sqrt(B2 * B2 - 4 * A * C2)) / (2 * A);
            double x4 = (-B2 - Math.Sqrt(B2 * B2 - 4 * A * C2)) / (2 * A);
            double y1 = kp * x1 + qp1;
            double y2 = kp * x2 + qp1;
            double y3 = kp * x3 + qp2;
            double y4 = kp * x4 + qp2;
            // Part 3: Getting parameters of parallel lines from evaluated points
            RealLineSegment upperParallelLine, lowerParallelLine;
            if (y1 < y2)
            {
                upperParallelLine = new RealLineSegment(k, y1 - k * x1, new RealCoordinate(x1, y1), new RealCoordinate(x3, y3));
                lowerParallelLine = new RealLineSegment(k, y2 - k * x2, new RealCoordinate(x2, y2), new RealCoordinate(x4, y4));
            }
            else
            {
                upperParallelLine = new RealLineSegment(k, y2 - k * x2, new RealCoordinate(x2, y2), new RealCoordinate(x4, y4));
                lowerParallelLine = new RealLineSegment(k, y1 - k * x1, new RealCoordinate(x1, y1), new RealCoordinate(x3, y3));
            }
            // Part 4: Getting parameters of perpendicular lines
            RealLineSegment upperPerpendicularLine, lowerPerpendicularLine;
            if (segment.start.y < segment.end.y)
            {
                upperPerpendicularLine = new RealLineSegment(kp, qp1, new RealCoordinate(segment.start.x, segment.start.y),
                    new RealCoordinate(x1, y1));
                lowerPerpendicularLine = new RealLineSegment(kp, qp2, new RealCoordinate(segment.end.x, segment.end.y),
                    new RealCoordinate(x3, y3));
            }
            else
            {
                upperPerpendicularLine = new RealLineSegment(kp, qp2, new RealCoordinate(segment.end.x, segment.end.y),
                    new RealCoordinate(x3, y3));
                lowerPerpendicularLine = new RealLineSegment(kp, qp1, new RealCoordinate(segment.start.x, segment.start.y),
                    new RealCoordinate(x1, y1));
            }
            // Part 5: Composing formulas 
            BoolExpr[] avoidingLines = new BoolExpr[pathSegments];
            NumberFormatInfo dot = new NumberFormatInfo();
            dot.NumberDecimalSeparator = ".";
            for (int i = 0; i < sourcesX.Length; i++)
            {
                // Part 5.1: Formulas for parallel lines
                ArithExpr sourcekupx_plus_q = ctx.MkAdd(ctx.MkMul(ctx.MkReal(upperParallelLine.k.ToString(dot)), sourcesX[i]), 
                    ctx.MkReal(upperParallelLine.q.ToString(dot)));
                BoolExpr sourceup_part = ctx.MkLe(sourcesY[i], sourcekupx_plus_q);
                ArithExpr destkupx_plus_q = ctx.MkAdd(ctx.MkMul(ctx.MkReal(upperParallelLine.k.ToString(dot)), destinationsX[i]),
                    ctx.MkReal(upperParallelLine.q.ToString(dot)));
                BoolExpr destup_part = ctx.MkLe(destinationsY[i], destkupx_plus_q);

                ArithExpr sourceklox_plus_q = ctx.MkAdd(ctx.MkMul(ctx.MkReal(lowerParallelLine.k.ToString(dot)), sourcesX[i]),
                    ctx.MkReal(lowerParallelLine.q.ToString(dot)));
                BoolExpr sourcelo_part = ctx.MkGe(sourcesY[i], sourceklox_plus_q);
                ArithExpr destklox_plus_q = ctx.MkAdd(ctx.MkMul(ctx.MkReal(lowerParallelLine.k.ToString(dot)), destinationsX[i]),
                    ctx.MkReal(lowerParallelLine.q.ToString(dot)));
                BoolExpr destlo_part = ctx.MkGe(destinationsY[i], destklox_plus_q);

                BoolExpr conjUpPar = ctx.MkAnd(sourceup_part, destup_part);
                BoolExpr conjLoPar = ctx.MkAnd(sourcelo_part, destlo_part);

                // Part 5.2: Formulas for perpendicular lines
                ArithExpr sourceperkupx_plus_q = ctx.MkAdd(ctx.MkMul(ctx.MkReal(upperPerpendicularLine.k.ToString(dot)),
                    sourcesX[i]), ctx.MkReal(upperPerpendicularLine.q.ToString(dot)));
                BoolExpr sourceperup_part = ctx.MkLe(sourcesY[i], sourceperkupx_plus_q);
                ArithExpr destperkupx_plus_q = ctx.MkAdd(ctx.MkMul(ctx.MkReal(upperPerpendicularLine.k.ToString(dot)),
                    destinationsX[i]), ctx.MkReal(upperPerpendicularLine.q.ToString(dot)));
                BoolExpr destperup_part = ctx.MkLe(destinationsY[i], destperkupx_plus_q);

                ArithExpr sourceperklox_plus_q = ctx.MkAdd(ctx.MkMul(ctx.MkReal(lowerPerpendicularLine.k.ToString(dot)),
                    sourcesX[i]), ctx.MkReal(lowerPerpendicularLine.q.ToString(dot)));
                BoolExpr sourceperlo_part = ctx.MkGe(sourcesY[i], sourceperklox_plus_q);
                ArithExpr destperklox_plus_q = ctx.MkAdd(ctx.MkMul(ctx.MkReal(lowerPerpendicularLine.k.ToString(dot)),
                    destinationsX[i]), ctx.MkReal(lowerPerpendicularLine.q.ToString(dot)));
                BoolExpr destperlo_part = ctx.MkGe(destinationsY[i], destperklox_plus_q);

                BoolExpr conjUpPer = ctx.MkAnd(sourceperup_part, destperup_part);
                BoolExpr conjLoPer = ctx.MkAnd(sourceperlo_part, destperlo_part);

                avoidingLines[i] = ctx.MkOr(conjUpPar, conjLoPar, conjUpPer, conjLoPer);
            }

            return ctx.MkAnd(avoidingLines);
        }

    }
}