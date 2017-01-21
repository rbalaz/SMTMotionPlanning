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
            if (startLocation.Is3DWorld)
                return find3DPath();
            else
                return find2DPath();
        }

        private Coordinate[] find2DPath()
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
            BoolExpr movementConstraints = generateMovementConstraint(ctx, sourcesX, sourcesY, destinationsX, destinationsY);
            BoolExpr prerequisitesConstraints = generateStartAndGoal(ctx, sourcesX, sourcesY, destinationsX, destinationsY);
            BoolExpr joiningPathSegments = joinPathSegments(ctx, sourcesX, sourcesY, destinationsX, destinationsY);
            BoolExpr avoidingObstacles = avoidObstacles(world, ctx, sourcesX, sourcesY, destinationsX, destinationsY);
            BoolExpr pathLengthConstraint = generatePathLengthConstraint(ctx, sourcesX, sourcesY, destinationsX, destinationsY);

            Solver s = ctx.MkSolver();
            s.Assert(worldSizeConstraints);
            if(!(curvedPath))
                s.Assert(movementConstraints);
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

        private BoolExpr generatePathLengthConstraint(Context ctx, IntExpr[] sourcesX, IntExpr[] sourcesY, IntExpr[] destinationsX, IntExpr[] destinationsY)
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
                        if(((RectangularObstacle)(world.obstacles[i])).realLocation != null)
                            obstacles.Add(handleRealRectangularObstacle((RectangularObstacle)world.obstacles[i], ctx, sourcesX, sourcesY, destinationsX, destinationsY));
                        else
                            obstacles.Add(handleRectangularObstacle((RectangularObstacle)world.obstacles[i], ctx, sourcesX, sourcesY, destinationsX, destinationsY));
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
    }
}
