// ;
using ExShared;
using Sandbox.Game;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;
using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;

namespace PocketShield
{
    public partial class Session_PocketShieldClient
    {
        //private Dictionary<long, OtherCharacterShieldData> m_ShieldDamageEffects = new Dictionary<long, OtherCharacterShieldData>();
        private List<OtherCharacterShieldData> m_DrawList = new List<OtherCharacterShieldData>();
        
        public override void Draw()
        {
            // gets called 60 times a second after all other update methods, regardless of framerate, game pause or MyUpdateOrder.
            // NOTE: this is the only place where the camera matrix (MyAPIGateway.Session.Camera.WorldMatrix) is accurate, everywhere else it's 1 frame behind.


            DrawShieldEffects();
        }

        private float radius = 1.20f;
        private MySimpleObjectRasterizer rasterization = MySimpleObjectRasterizer.SolidAndWireframe;
        private int wireDivideRatio = 20;
        private float lineThickness = 0.004f;
        private float intensity = 5;

        //float percent = 0.0f;
        //int timer = 0;
        private void DrawShieldEffects()
        {
            if (MyAPIGateway.Session == null)
                return;

            IMyEntity targetEntity = null;

            //m_DrawList.Clear();
            //m_DrawList.Add(new OtherCharacterShieldData()
            //{
            //    EntityId = MyAPIGateway.Session.Player.Character.EntityId,
            //});

            Color drawColor = Color.White;

            foreach (var data in m_DrawList)
            {
                targetEntity = MyAPIGateway.Entities.GetEntityById(data.EntityId);
                if (targetEntity == null)
                    continue;

                //GenerateColorFromGradientPercent(out drawColor, percent, (float)timer / Constants.HIT_EFFECT_TICKS);
                GenerateColorFromGradientPercent(out drawColor, data.ShieldAmountPercent, (float)data.Ticks / Constants.HIT_EFFECT_TICKS);
                var characterMatrix = targetEntity.WorldMatrix;
                var adjMatrix = MatrixD.CreateWorld(characterMatrix.Up * 1.0 + targetEntity.GetPosition(), characterMatrix.Down, characterMatrix.Forward);

                try
                {
                    MySimpleObjectDraw.DrawTransparentSphere(ref adjMatrix, radius, ref drawColor, rasterization, wireDivideRatio, 
                        MyStringId.GetOrCompute("Square"), MyStringId.GetOrCompute("Square"), lineThickness, 
                        blendType: BlendTypeEnum.Standard, intensity: intensity);
                    
                }
                catch (Exception _e)
                {
                    ClientLogger.Log("  > Exception < Error during drawing shield effects: " + _e.Message, 0);
                }
                
                if (data.ShouldPlaySound)
                {
                    MyVisualScriptLogicProvider.PlaySingleSoundAtPosition("ArcParticleElectricalDischarge", targetEntity.GetPosition());
                    data.ShouldPlaySound = false;
                }
                
            }


            
        }
        
        public void GenerateColorFromGradientPercent(out Color _color, float _percent, float _alphaPercent)
        {

#if true
            float hue = (_percent * 180.0f) / 360.0f;
            _color = ColorExtensions.HSVtoColor(new Vector3(hue, 1.0f, 1.0f));
            //ClientLogger.Log("percent = " + _percent + ", hue = " + hue + ", color = " + _color.ToString());

            //float baseAlpha = MathHelper.Lerp(48.0f, 48.0f, _percent);
            _color.A = (byte)(_alphaPercent * 223.0f);
#else
            // R FF FF 00
            // G 00 FF FF
            // B 00 00 FF
            int r, g, b, a;

            if (_percent < 0.5f)
            {
                r = 255;
                b = 0;
            }
            else
            {
                r = (int)MathHelper.Lerp(255.0f, 0.0f, (_percent - 0.5f) / 0.5f);
                b = (int)MathHelper.Lerp(0.0f, 255.0f, (_percent - 0.5f) / 0.5f);
            }

            if (_percent > 0.5f)
                g = 255;
            else
                g = (int)MathHelper.Lerp(0.0f, 255.0f, _percent / 0.5f);

            float baseAlpha = MathHelper.Lerp(48.0f, 48.0f, _percent);
            a = (int)MathHelper.Lerp(baseAlpha, 191, _alphaPercent);

            _color = Color.FromNonPremultiplied(r, g, b, a);
#endif
            


        }
    }
}
