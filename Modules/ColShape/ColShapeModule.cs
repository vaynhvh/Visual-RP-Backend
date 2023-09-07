using Backend.Models;
using Backend.Utils;
using Backend.Utils.Extensions;
using GTANetworkAPI;
//using PostSharp.Extensibility;
//using PostSharp.Patterns.Diagnostics;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Modules.ColShape
{
    //[Log(AttributeTargetMemberAttributes = MulticastAttributes.Public)]

    public enum ColShapeKeyType
    {
        E,
        L,
        BOTH
    }
    class ColShapeModule : RXModule
    {
        public ColShapeModule() : base("ColShape") { }

        //[HandleExceptions]
        public override async Task OnColShape(RXColShape shape, RXPlayer player, bool state)
        {
            if (shape.Message != null && state && shape.Message.Text.Length > 0)
            {
                if ((shape.Message.RestrictedToTeam != 0 && shape.Message.RestrictedToTeam != player.TeamId) || (shape.Message.OnlyBadFaction && (player.TeamId == 0 || !player.Team.IsGangster()))) return;


                await player.SendNotify(shape.Message.Text, shape.Message.Duration, shape.Message.Color, shape.Message.Title);
            }
        }

        //[HandleExceptions]
        public override async Task PressedL(RXPlayer player)
        {
            Vector3 pos = await player.GetPositionAsync();
            NAPI.Task.Run(async () =>
            {

                List<RXColShape> colShapes = NAPI.Pools.GetAllColShapes().Cast<RXColShape>().Where(x => !x.IsContainerColShape && x.ColShapeKeyType == ColShapeKeyType.BOTH || x.ColShapeKeyType == ColShapeKeyType.L).ToList();

                foreach (RXColShape shape in colShapes.ToList())
                {

                    if (shape.Dimension != player.Dimension)
                    {
                        colShapes.Remove(shape);
                    }

                }

                if (colShapes == null || colShapes.Count < 1) return;
                RXColShape colShape = colShapes.FirstOrDefault(colShape => colShape.IsPointWithin(player.Position));
                if (colShape == null) return;


                if (colShape.Dimension != player.Dimension && colShape.Dimension != UInt32.MaxValue) return;
                if (!colShape.IsInteractionColShape) return;

                await NAPI.Task.RunAsync(() => colShape.Action.Invoke(player));

            });
        }
        public override async Task PressedE(RXPlayer player)
        {
            Vector3 pos = await player.GetPositionAsync();
            NAPI.Task.Run(async () =>
            {

                List<RXColShape> colShapes = NAPI.Pools.GetAllColShapes().Cast<RXColShape>().Where(x => !x.IsContainerColShape && x.ColShapeKeyType == ColShapeKeyType.BOTH || x.ColShapeKeyType == ColShapeKeyType.E).ToList();

                foreach (RXColShape shape in colShapes.ToList())
                {

                    if (shape.Dimension != player.Dimension)
                    {
                        colShapes.Remove(shape);
                    }

                }

                if (colShapes == null || colShapes.Count < 1) return;
                List<RXColShape> colShape = colShapes.Where(x => x.IsPointWithin(player.Position)).ToList();

                foreach (var shape in colShape) {
                    if (shape == null) continue;


                    if (shape.Dimension != player.Dimension && shape.Dimension != UInt32.MaxValue) continue;
                    if (!shape.IsInteractionColShape) continue;

                    await NAPI.Task.RunAsync(() => shape.Action.Invoke(player));

                }
              

            });
        }
    }
}
