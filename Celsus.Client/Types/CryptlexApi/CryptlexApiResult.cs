namespace Celsus.Client.Types.CryptlexApi
{
    public class CryptlexApiResult<T>
    {
        public CryptlexError Error { get; set; }

        public T Result { get; set; }
    }

}
