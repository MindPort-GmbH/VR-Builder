namespace VRBuilder.Core.IO
{
    public class ProcessAssetData
    {
        public byte[] Data { get; private set; }
        public string FileName { get; private set; }

        public ProcessAssetData(byte[] data, string fileName)
        {
            Data = data;
            FileName = fileName;
        }
    }
}
