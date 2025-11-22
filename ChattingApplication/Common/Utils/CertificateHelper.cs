using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace ChattingApplication.Common.Utils;

public static class CertificateHelper
{
  public static X509Certificate2 GenerateSelfSignedCertificate(string subjectName, string password)
  {
    using var rsa = RSA.Create(2048);

    var request = new CertificateRequest(
      $"CN={subjectName}",
      rsa,
      HashAlgorithmName.SHA256,
      RSASignaturePadding.Pkcs1);

    var certificate = request.CreateSelfSigned(
      DateTimeOffset.Now,
      DateTimeOffset.Now.AddYears(1));

    var pfxBytes = certificate.Export(X509ContentType.Pfx, password);

    return X509CertificateLoader.LoadPkcs12(
      pfxBytes,
      password,
    X509KeyStorageFlags.Exportable | X509KeyStorageFlags.UserKeySet
    );
  }
}
