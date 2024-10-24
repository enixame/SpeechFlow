using Whisper.net.Ggml;

namespace SpeechFlowCsharp.EnumsMapping
{
    public static class GgmlTypeStringMapper
    {
        public static string ToGgmlString(GgmlType ggmlType)
        {
            return ggmlType switch
            {
                GgmlType.Tiny => "models/ggml-tiny.bin",
                GgmlType.Base => "models/ggml-base.bin",
                GgmlType.Medium => "models/ggml-medium.bin",
                GgmlType.Small => "models/ggml-small.bin",
                GgmlType.LargeV1 => "models/ggml-large-v1.bin",
                GgmlType.LargeV2 => "models/ggml-large-v2.bin",
                GgmlType.LargeV3 => "models/ggml-large-v3.bin",
                GgmlType.LargeV3Turbo => "models/ggml-large-v3-turbo.bin",
                _ => throw new ArgumentOutOfRangeException(nameof(ggmlType)),
            };
        }
    }
}