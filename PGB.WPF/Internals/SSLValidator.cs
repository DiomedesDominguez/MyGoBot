namespace PGB.WPF.Internals
{
    using System.Net;
    using System.Net.Security;

    internal static class SSLValidator
    {
        private static RemoteCertificateValidationCallback _defaultCallback;
        public static RemoteCertificateValidationCallback OnValidateCertificate;

        public static void OverrideValidation()
        {
            _defaultCallback = ServicePointManager.ServerCertificateValidationCallback;
            ServicePointManager.ServerCertificateValidationCallback = OnValidateCertificate;
        }

        public static void RestoreValidation()
        {
            ServicePointManager.ServerCertificateValidationCallback = _defaultCallback;
        }
    }
}