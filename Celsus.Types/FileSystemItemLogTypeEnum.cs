namespace Celsus.Types
{
    public enum FileSystemItemLogTypeEnum
    {
        TesseractOcrOk = 1,
        Finished = 2,
        SaveTextDataOk = 3,


        StoppedWithError = 1001,
        SaveTextDataFileNotFound = 1002,
        Info = 1003,
        Error = 1004
    }
}