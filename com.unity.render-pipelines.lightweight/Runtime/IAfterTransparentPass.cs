namespace UnityEngine.Rendering.LWRP
{
    public interface IAfterTransparentPass
    {
        ScriptableRenderPass GetPassToEnqueue(RenderTextureDescriptor baseDescriptor, RenderTargetHandle colorHandle, RenderTargetHandle depthHandle);
    }
}
