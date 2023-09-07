using Backend.Models;
using GTANetworkAPI;
//using PostSharp.Extensibility;
//using PostSharp.Patterns.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Utils.Extensions
{
    public class MCB
    {
        public RXColShape ColShape { get; set; }
        public Blip Blip { get; set; }
        public Marker Marker { get; set; }

        public MCB() { }
    }

    //[Log(AttributeTargetMemberAttributes = MulticastAttributes.Public)]
    internal static class EntityExtensions
    {
        //[HandleExceptions]
        public async static Task<MCB> CreateMCB(this GTANetworkMethods.Entity entity, Vector3 position, Color color, uint dimension = UInt32.MaxValue, float colShapeRange = 1.4f, float colShapeHeight = 2.4f, bool marker = false, MarkerType markerType = MarkerType.VerticalCylinder, bool blip = false, int blipType = 1, byte blipColor = 0, string name = "")
        {
            MCB mcb = new MCB();

            var colShape = await NAPI.Task.RunReturnAsync(() => (RXColShape)NAPI.ColShape.CreateCylinderColShape(position, colShapeRange, colShapeHeight, dimension));
            colShape.IsInteractionColShape = true;

            mcb.ColShape = colShape;

            if (marker)
            {
                var markerObj = await NAPI.Task.RunReturnAsync(() => NAPI.Marker.CreateMarker(markerType, position - new Vector3(0, 0, 1), new Vector3(), new Vector3(), colShapeRange, color, false, dimension));

                mcb.Marker = markerObj;
            }

            if (blip)
            {
                var blipObj = await NAPI.Task.RunReturnAsync(() => NAPI.Blip.CreateBlip(blipType, position, 1.0f, blipColor, name, 255, 0, true, 0, dimension));

                mcb.Blip = blipObj;
            }

            return mcb;
        }

        //[HandleExceptions]
        public async static Task<MCB> CreateMCB3(this GTANetworkMethods.Entity entity, Vector3 position, Color color, uint dimension = UInt32.MaxValue, float colShapeRange = 1.4f, float colShapeHeight = 2.4f, bool marker = false, MarkerType markerType = MarkerType.VerticalCylinder, bool blip = false, int blipType = 1, byte blipColor = 0, string name = "")
        {
            MCB mcb = new MCB();

            var colShape = await NAPI.Task.RunReturnAsync(() => (RXColShape)NAPI.ColShape.CreateCylinderColShape(position, colShapeRange, colShapeHeight, dimension));
            colShape.IsInteractionColShape = true;

            mcb.ColShape = colShape;

            if (marker)
            {
                var markerObj = await NAPI.Task.RunReturnAsync(() => NAPI.Marker.CreateMarker(markerType, position - new Vector3(0, 0, 1), new Vector3(), new Vector3(), colShapeRange * 2, color, false, dimension));

                mcb.Marker = markerObj;
            }

            if (blip)
            {
                var blipObj = await NAPI.Task.RunReturnAsync(() => NAPI.Blip.CreateBlip(blipType, position, 1.0f, blipColor, name, 255, 0, true, 0, dimension));

                mcb.Blip = blipObj;
            }

            return mcb;
        }

        //[HandleExceptions]
        public async static Task<MCB> CreateMCB2(this GTANetworkMethods.Entity entity, Vector3 position, Color color, uint dimension = UInt32.MaxValue, float colShapeRange = 1.4f, float colShapeHeight = 2.4f, float markerSize = 1.0f, bool marker = false, MarkerType markerType = MarkerType.VerticalCylinder, bool blip = false, int blipType = 1, byte blipColor = 0, string name = "")
        {
            MCB mcb = new MCB();

            var colShape = await NAPI.Task.RunReturnAsync(() => (RXColShape)NAPI.ColShape.CreateCylinderColShape(position, colShapeRange, colShapeHeight, dimension));
            colShape.IsInteractionColShape = true;

            mcb.ColShape = colShape;

            if (marker)
            {
                var markerObj = await NAPI.Task.RunReturnAsync(() => NAPI.Marker.CreateMarker(markerType, position - new Vector3(0, 0, 1), new Vector3(), new Vector3(), markerSize, color, false, dimension));

                mcb.Marker = markerObj;
            }

            if (blip)
            {
                var blipObj = await NAPI.Task.RunReturnAsync(() => NAPI.Blip.CreateBlip(blipType, position, 1.0f, blipColor, name, 255, 0, true, 0, dimension));

                mcb.Blip = blipObj;
            }

            return mcb;
        }
    }
}
