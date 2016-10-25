using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Z3;


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

            BoolExpr[] checkersX = new BoolExpr[pathSegments];
            BoolExpr[] checkersY = new BoolExpr[pathSegments];
            for (int i = 0; i < pathSegments; i++)
            {
                checkersX[i] = ctx.MkBoolConst("psix" + i);
                checkersY[i] = ctx.MkBoolConst("psiy" + i);
            }

            IntExpr[] obstacleLeftX = new IntExpr[world.getObstacles().Count];
            IntExpr[] obstacleRightX = new IntExpr[world.getObstacles().Count];
            IntExpr[] obstacleLeftY = new IntExpr[world.getObstacles().Count];
            IntExpr[] obstacleRightY = new IntExpr[world.getObstacles().Count];
            for (int i = 0; i < obstacleLeftX.Length; i++)
            {
                obstacleLeftX[i] = ctx.MkInt(world.getObstacles()[i].getLocation().getX());
                obstacleRightX[i] = ctx.MkInt(world.getObstacles()[i].getLocation().getX() + world.getObstacles()[i].getWidth());
                obstacleLeftY[i] = ctx.MkInt(world.getObstacles()[i].getLocation().getY());
                obstacleRightY[i] = ctx.MkInt(world.getObstacles()[i].getLocation().getY() + world.getObstacles()[i].getLength());
            }

            IntExpr zero = ctx.MkInt("0");
            IntExpr worldLength = ctx.MkInt(world.getLength());
            IntExpr worldWidth = ctx.MkInt(world.getWidth());
            IntExpr startX = ctx.MkInt(startLocation.getX());
            IntExpr startY = ctx.MkInt(startLocation.getY());
            IntExpr goalX = ctx.MkInt(goalLocation.getX());
            IntExpr goalY = ctx.MkInt(goalLocation.getY());
            IntExpr passDistance = ctx.MkInt(obstaclePassDistance);

            // First input into SMT solver: Field of constrainsConj.
            // Contains constrains for movement so that agent will not try to move out of world, surpassing one its parameters
            // Fix 14.10.2016: Expressions lacked any logical symbols to join them in a conjunction
            BoolExpr[] constrains = new BoolExpr[8 * pathSegments];
            for (int i = 0; i < constrains.Length; i = i + 8)
            {
                constrains[i] = ctx.MkLe(zero, sourcesX[i / 8]);
                constrains[i + 1] = ctx.MkLe(sourcesX[i / 8], worldWidth);
                constrains[i + 2] = ctx.MkLe(zero, sourcesY[i / 8]);
                constrains[i + 3] = ctx.MkLe(sourcesY[i / 8], worldLength);
                constrains[i + 4] = ctx.MkLe(zero, destinationsX[i / 8]);
                constrains[i + 5] = ctx.MkLe(destinationsX[i / 8], worldWidth);
                constrains[i + 6] = ctx.MkLe(zero, destinationsY[i / 8]);
                constrains[i + 7] = ctx.MkLe(destinationsY[i / 8], worldLength);
            }
            BoolExpr constrainsConj = ctx.MkAnd(constrains);


            // Second input into SMT solver: field of bothDogsConj
            // One of the still unknown properties for me about this task.
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
            BoolExpr bothDogsConj = ctx.MkAnd(bothDogs);

            // Third input into SMT solver: prerequisites expression
            // These expressions only mark the start and goal location in the sources and destinations fields
            BoolExpr initialXPosition = ctx.MkEq(sourcesX[0], startX);
            BoolExpr initialYPosition = ctx.MkEq(sourcesY[0], startY);
            BoolExpr goalXPosition = ctx.MkEq(destinationsX[pathSegments - 1], goalX);
            BoolExpr goalYPosition = ctx.MkEq(destinationsY[pathSegments - 1], goalY);
            BoolExpr prerequisites = ctx.MkAnd(new BoolExpr[] { initialXPosition, initialYPosition, goalXPosition, goalYPosition });

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
            BoolExpr pathSegmentsConj = ctx.MkAnd(pathSegmentsXConj, pathSegmentsYConj);

            // Fifth input into SMT solver: avoidingObstaclesCombinedConj field
            // Defines the way on how agent is supposed to avoid obstacles, including distance it needs to keep when 
            // moving around them
            // Fix1 14.10.2016: avoidingObstacles field was incomplete due to lack of index logic inside j cycle, which caused
            // overwriting old expressions with new instead of appending them
            // Fix2 14.10.2016: fixed sizes of fields and ranges of for cycle as it was causing indexoutofbounds exceptions
            // after applying previous fix
            // Fix3 14.10.2016: fixed wrong logic in relating expressions within the expression fields
            BoolExpr[] avoidingObstaclesX = new BoolExpr[pathSegments * world.getObstacles().Count];
            BoolExpr[] avoidingObstaclesY = new BoolExpr[pathSegments * world.getObstacles().Count];
            BoolExpr[] avoidingObstaclesCombined = new BoolExpr[pathSegments * world.getObstacles().Count];
            for (int i = 0; i < pathSegments; i++)
            {
                for (int j = 0; j < world.getObstacles().Count; j++)
                {
                    ArithExpr xl_minus_xs = ctx.MkSub(obstacleLeftX[j], sourcesX[i]);
                    ArithExpr xl_minus_xd = ctx.MkSub(obstacleLeftX[j], destinationsX[i]);
                    ArithExpr xs_minus_xh = ctx.MkSub(sourcesX[i], obstacleRightX[j]);
                    ArithExpr xd_minus_xh = ctx.MkSub(destinationsX[i], obstacleRightX[j]);
                    avoidingObstaclesX[i * world.getObstacles().Count + j] = ctx.MkOr(ctx.MkAnd(ctx.MkGe(xl_minus_xs, passDistance), ctx.MkGe(xl_minus_xd, passDistance)),
                        ctx.MkAnd(ctx.MkGe(xs_minus_xh, passDistance), ctx.MkGe(xd_minus_xh, passDistance)));

                    ArithExpr yl_minus_ys = ctx.MkSub(obstacleLeftY[j], sourcesY[i]);
                    ArithExpr yl_minus_yd = ctx.MkSub(obstacleLeftY[j], destinationsY[i]);
                    ArithExpr ys_minus_yh = ctx.MkSub(sourcesY[i], obstacleRightY[j]);
                    ArithExpr yd_minus_yh = ctx.MkSub(destinationsY[i], obstacleRightY[j]);
                    avoidingObstaclesY[i * world.getObstacles().Count + j] = ctx.MkOr(ctx.MkAnd(ctx.MkGe(yl_minus_ys, passDistance), ctx.MkGe(yl_minus_yd, passDistance)),
                        ctx.MkAnd(ctx.MkGe(ys_minus_yh, passDistance), ctx.MkGe(yd_minus_yh, passDistance)));
                    avoidingObstaclesCombined[i * world.getObstacles().Count + j] = ctx.MkOr(avoidingObstaclesX[i * world.getObstacles().Count + j],
                        avoidingObstaclesY[i * world.getObstacles().Count + j]);
                }
            }
            BoolExpr avoidingObstaclesCombinedConj = ctx.MkAnd(avoidingObstaclesCombined);

            // Potentially sixth input into SMT solver is the path length constraint

            Solver s = ctx.MkSolver();
            // Fix 14.10.2016: Fixed wrong asserting of partial, unrelated or unfinished expressions 
            s.Assert(constrainsConj);
            s.Assert(bothDogsConj);
            s.Assert(prerequisites);
            s.Assert(pathSegmentsConj);
            s.Assert(avoidingObstaclesCombinedConj);
            // 14.10.2016 12.04: First compilation with model satisfiability confirmed.
            if (s.Check() != Status.SATISFIABLE)
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
                String s = expr.ToString();
                int output;
                Int32.TryParse(s, out output);
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
    }
}
