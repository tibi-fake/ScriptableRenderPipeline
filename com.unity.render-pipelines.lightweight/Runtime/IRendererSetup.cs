namespace UnityEngine.Rendering.LWRP
{
    public interface IRendererSetup
    {
        void Setup(ScriptableRenderer renderer, ref RenderingData renderingData);
    }
}
