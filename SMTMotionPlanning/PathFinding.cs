using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Z3;
using System.Globalization;

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

        public PathFinding(Coordinate startLocation, Coordinate goalLocation, int pathSegments, int obstaclePassDistance, Space world)
        {
            this.startLocation = startLocation;
            this.goalLocation = goalLocation;
            this.pathSegments = pathSegments;
            this.obstaclePassDistance = obstaclePassDistance;
            this.world = world;
        }

        public Coordinate[] findPath()
        {
            if (startLocation.Is3DWorld)
                return find3DPath();
            else
                return find2DPath();
        }

        private Coordinate[] find2DPath()
        {
            Coordinate[] path = new Coordinate[pathSegments + 1];

            Context ctx = new Context();

            // Fix 14.10.2016: Fixed conflicting names of source and destination variables
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
            BoolExpr movementConstraints = generateMovementConstraint(ctx, sourcesX, sourcesY, destinationsX, destinationsY);
            BoolExpr prerequisitesConstraints = generateStartAndGoal(ctx, sourcesX, sourcesY, destinationsX, destinationsY);
            BoolExpr joiningPathSegments = joinPathSegments(ctx, sourcesX, sourcesY, destinationsX, destinationsY);
            BoolExpr avoidingObstacles = avoidObstacles(world, ctx, sourcesX, sourcesY, destinationsX, destinationsY);

            Solver s = ctx.MkSolver();
            // Fix 14.10.2016: Fixed wrong asserting of partial, unrelated or unfinished expressions 
            s.Assert(worldSizeConstraints);
            s.Assert(movementConstraints);
            s.Assert(prerequisitesConstraints);
            s.Assert(joiningPathSegments);
            s.Assert(avoidingObstacles);
            // 14.10.2016 12.04: First compilation with model satisfiability confirmed.
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

            return path;
        }

        private int convertExprToInt(Expr expr)
        {
            if (expr.IsIntNum)
            {
                string s = expr.ToString();
                int output;
                int.TryParse(s, out output);
                return output;
            }
            else
                throw new IllegalConversionException();
        }

        private Coordinate[] find3DPath()
        {
            /* Coordinate[] path;

            return path;*/
            throw new NotImplementedException();
        }

        private BoolExpr generateWorldSizeConstraints(Context ctx, IntExpr[] sourcesX, IntExpr[] sourcesY, IntExpr[] destinationsX, IntExpr[] destinationsY)
        {
            // First input into SMT solver: Field of constrainsConj.
            // Contains constrains for movement so that agent will not try to move out of world, surpassing one its parameters
            // Fix 14.10.2016: Expressions lacked any logical symbols to join them in a conjunction
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

        private BoolExpr generateMovementConstraint(Context ctx, IntExpr[] sourcesX, IntExpr[] sourcesY, IntExpr[] destinationsX, IntExpr[] destinationsY)
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
                bothDogs[i] = ctx.MkOr(dogX[i], dogY[i]);
            }

            return ctx.MkAnd(bothDogs);
        }

        private BoolExpr generateStartAndGoal(Context ctx, IntExpr[] sourcesX, IntExpr[] sourcesY, IntExpr[] destinationsX, IntExpr[] destinationsY)
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

        private BoolExpr joinPathSegments(Context ctx, IntExpr[] sourcesX, IntExpr[] sourcesY, IntExpr[] destinationsX, IntExpr[] destinationsY)
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

        private BoolExpr pathLength()
        {
            // Potentially sixth input into SMT solver is the path length constraint
            throw new NotImplementedException();
        }

        private BoolExpr avoidObstacles(Space world, Context ctx, IntExpr[] sourcesX, IntExpr[] sourcesY, IntExpr[] destinationsX, IntExpr[] destinationsY)
        {
            // Fifth input into SMT solver: avoidingObstaclesCombinedConj field
            // Defines the way on how agent is supposed to avoid obstacles, including distance it needs to keep when 
            // moving around them
            List<BoolExpr> obstacles = new List<BoolExpr>();
            for (int i = 0; i < world.obstacles.Count; i++)
            {
                switch (world.obstacles[i].type)
                {
                    case Obstacle.ObstacleType.Ellipse:
                        obstacles.Add(handleEllipticalObstacle((EllipticalObstacle)world.obstacles[i], ctx, destinationsX, destinationsY, sourcesX, sourcesY));
                        break;
                    case Obstacle.ObstacleType.Rectangle:
                        obstacles.Add(handleRectangularObstacle((RectangularObstacle)world.obstacles[i], ctx, sourcesX, sourcesY, destinationsX, destinationsY));
                        break;
                    case Obstacle.ObstacleType.Polygon:
                        obstacles.Add(handlePolygonalObstacle((PolygonalObstacle)world.obstacles[i], ctx, sourcesX, sourcesY, destinationsX, destinationsY));
                        break;
                    case Obstacle.ObstacleType.Spline:
                        obstacles.Add(handleSplineObstacle((SplineObstacle)world.obstacles[i], ctx, sourcesX, sourcesY, destinationsX, destinationsY));
                        break;
                }
            }

            return ctx.MkAnd(obstacles.ToArray());
        }

        private BoolExpr handleEllipticalObstacle(EllipticalObstacle obstacle, Context ctx, IntExpr[] destinationsX, IntExpr[] destinationsY, IntExpr[] sourcesX, IntExpr[] sourcesY)
        {
            RectangularObstacle rectangle = new RectangularObstacle(obstacle.length, obstacle.width, 
                new Coordinate(obstacle.location.x - obstacle.width/2, obstacle.location.y - obstacle.length/2));
            return handleRectangularObstacle(rectangle, ctx, sourcesX, sourcesY, destinationsX, destinationsY);
        }

        private BoolExpr handleRectangularObstacle(RectangularObstacle obstacle, Context ctx, IntExpr[] sourcesX, IntExpr[] sourcesY, IntExpr[] destinationsX, IntExpr[] destinationsY)
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

        private BoolExpr handleRealRectangularObstacle(RectangularObstacle obstacle, Context ctx, IntExpr[] sourcesX, IntExpr[] sourcesY, IntExpr[] destinationsX, IntExpr[] destinationsY)
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

        private BoolExpr handlePolygonalObstacle(PolygonalObstacle obstacle, Context ctx, IntExpr[] sourcesX, IntExpr[] sourcesY, IntExpr[] destinationsX, IntExpr[] destinationsY)
        {
            List<LineSegment> lines = obstacle.getLines();
            List<BoolExpr> avoidingLines = new List<BoolExpr>();
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].isHorizontal)
                {
                    int leftX = (lines[i].start.x < lines[i].end.x ? lines[i].start.x : lines[i].end.x) - obstaclePassDistance;
                    int leftY = lines[i].start.y - obstaclePassDistance;
                    RectangularObstacle rectangle = new RectangularObstacle(Coordinate.getXDistanceBetweenCoordinates
                        (lines[i].start, lines[i].end) + 2 * obstaclePassDistance, 2 * obstaclePassDistance, new Coordinate(leftX, leftY));
                    avoidingLines.Add(handleRectangularObstacle(rectangle, ctx, sourcesX, sourcesY, destinationsX, destinationsY));
                }
                else if (lines[i].isVertical)
                {
                    int leftY = (lines[i].start.y < lines[i].end.y ? lines[i].start.y : lines[i].end.y) - obstaclePassDistance;
                    int leftX = lines[i].start.x - obstaclePassDistance;
                    RectangularObstacle rectangle = new RectangularObstacle(2 * obstaclePassDistance,
                        Coordinate.getYDistanceBetweenCoordinates(lines[i].start, lines[i].end) + 2 * obstaclePassDistance, new Coordinate(leftX, leftY));
                    avoidingLines.Add(handleRectangularObstacle(rectangle, ctx, sourcesX, sourcesY, destinationsX, destinationsY));
                }
                else
                {
                    List<RectangularObstacle> segmentCases = encaseLineSegment(lines[i]);
                    foreach (RectangularObstacle rec in segmentCases)
                        avoidingLines.Add(handleRealRectangularObstacle(rec, ctx, sourcesX, sourcesY, destinationsX, destinationsY));
                }
            }
            return ctx.MkAnd(avoidingLines.ToArray());
        }

        private BoolExpr handleSplineObstacle(SplineObstacle obstacle, Context ctx, IntExpr[] sourcesX, IntExpr[] sourcesY, IntExpr[] destinationsX, IntExpr[] destinationsY)
        {
            if (obstacle.points.Count > 4)
            {
                SplineObstacle firstSegment = new SplineObstacle(obstacle.points.Take(3).ToList());
                SplineObstacle secondSegment = new SplineObstacle(obstacle.points.Skip(3).ToList());
                BoolExpr first = handleSplineObstacle(firstSegment, ctx, sourcesX, sourcesY, destinationsX, destinationsY);
                BoolExpr second = handleSplineObstacle(secondSegment, ctx, sourcesX, sourcesY, destinationsX, destinationsY);
                return ctx.MkAnd(first, second);
            }
            else
            {
                double[] x = obstacle.points.Select(item => (double)item.x).ToArray();
                double[] y = obstacle.points.Select(item => (double)item.y).ToArray();

                Polynom poly = new Polynom(SplineObstacle.Polyfit(x, y, obstacle.points.Count - 1).Reverse().ToArray());
                RealCoordinate current = new RealCoordinate(obstacle.points[0].x, obstacle.points[0].y - obstaclePassDistance);
                RealCoordinate end = new RealCoordinate(obstacle.points[obstacle.points.Count - 1].x,
                    obstacle.points[obstacle.points.Count - 1].y);
                List<RectangularObstacle> obstacles = new List<RectangularObstacle>();
                RectangularObstacle firstObstacle = new RectangularObstacle(2 * obstaclePassDistance, obstaclePassDistance,
                    new RealCoordinate(current.x - obstaclePassDistance, current.y - obstaclePassDistance));
                obstacles.Add(firstObstacle);
                do
                {
                    RectangularObstacle rect = new RectangularObstacle(2 * obstaclePassDistance, 2,
                        new RealCoordinate(current.x, current.y));
                    obstacles.Add(rect);
                    if (current.x + 2 <= end.x)
                    {
                        current.x += 2;
                        current.y = poly.getPolynomValue(current.x) - obstaclePassDistance;
                    }
                    else
                    {
                        current.x = end.x;
                        current.y = end.y;
                    }
                } while (RealCoordinate.getDistanceBetweenCoordinates(current, end) != 0);
                RectangularObstacle lastObstacle = new RectangularObstacle(2 * obstaclePassDistance, obstaclePassDistance,
                    new RealCoordinate(current.x, current.y));
                obstacles.Add(lastObstacle);

                List<BoolExpr> avoidingObstacles = new List<BoolExpr>();
                foreach (RectangularObstacle rect in obstacles)
                {
                    avoidingObstacles.Add(handleRealRectangularObstacle(rect, ctx, sourcesX, sourcesY, destinationsX, destinationsY));
                }

                return ctx.MkAnd(avoidingObstacles.ToArray());
            }
        }

        private List<RectangularObstacle> encaseLineSegment(LineSegment segment)
        {
            double x = segment.start.x < segment.end.x ? segment.start.x : segment.end.x;
            double y = segment.start.x < segment.end.x ? segment.start.y : segment.end.y;
            double otherSideX = x == segment.start.x ? segment.end.x : segment.start.x;
            double otherSideY = y == segment.start.y ? segment.end.y : segment.start.y;
            RealCoordinate current = new RealCoordinate(x, y - obstaclePassDistance);
            RealCoordinate realEnd = new RealCoordinate(otherSideX, otherSideY);
            List<RectangularObstacle> obstacles = new List<RectangularObstacle>();
            RectangularObstacle firstObstacle = new RectangularObstacle(2 * obstaclePassDistance, obstaclePassDistance,
                new RealCoordinate(x - obstaclePassDistance, y - obstaclePassDistance));
            obstacles.Add(firstObstacle);
            do
            {
                RectangularObstacle obstacle = new RectangularObstacle(2 * obstaclePassDistance, 2, 
                    new RealCoordinate(current.x, current.y));
                obstacles.Add(obstacle);
                if (current.x + 2 <= otherSideX)
                {
                    current.x += 2;
                    current.y = segment.k * current.x + segment.q - obstaclePassDistance;
                }
                else
                {
                    current.x = otherSideX;
                    current.y = otherSideY;
                }
            } while (RealCoordinate.getDistanceBetweenCoordinates(current, realEnd) != 0);
            RectangularObstacle lastObstacle = new RectangularObstacle(2 * obstaclePassDistance, obstaclePassDistance,
                new RealCoordinate(current.x, current.y));
            obstacles.Add(lastObstacle);

            return obstacles;
        }
    }
}
