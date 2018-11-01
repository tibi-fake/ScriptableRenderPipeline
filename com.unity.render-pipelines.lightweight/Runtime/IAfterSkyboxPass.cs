namespace UnityEngine.Rendering.LWRP
{
    public interface IAfterSkyboxPass
    {
        ScriptableRenderPass GetPassToEnqueue(RenderTextureDescriptor baseDescriptor, RenderTargetHandle colorHandle, RenderTargetHandle depthHandle);
    }
}
