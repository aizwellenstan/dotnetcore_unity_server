using System;
using System.Collections.Generic;
using System.Reflection;
using ETModel;
using JWT;
using JWT.Algorithms;
using JWT.Exceptions;
using JWT.Serializers;

namespace ETHotfix
{
    /// <summary>
    /// 只有Server會用，所以Key跟IV特別寫在這，為了達到類似Token的效果
    /// </summary>
	public static class SignInCryptographyHelper
    {
	    private const string cKey = "pkJCeiVpZzZqO6lyVmJDC2AnVltWNVomJn44RSZ4UWU=";

	    private const string cIV = "cAAaRfj4iHh==99fyGDbPA==";

	    private static byte[] _key;

	    private static byte[] key
	    {
	        get
	        {
	            if (_key == null)
	            {
	                _key = CryptographyHelper.GenerateKey(cKey);

	            }
	            return _key;
	        }
	    }

        private static byte[] _IV;

	    private static byte[] IV
        {
	        get
	        {
	            if (_IV == null)
	            {
	                _IV = CryptographyHelper.GenerateIV(cIV);
	            }
	            return _IV;
	        }
	    }

        public static string Encrypt(string raw)
        {
            return CryptographyHelper.AESEncrypt(key, IV, raw);
        }

        public static string Decrypt(string source)
        {
            return CryptographyHelper.AESDecrypt(key, IV, source);
        }

        /// <summary>
        /// 用JWT套件編碼Token
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        public static string EncodeToken(Token payload)
        {
            try
            {
                IJwtAlgorithm algorithm = new HMACSHA256Algorithm(); // symmetric
                IJsonSerializer serializer = new JsonNetSerializer();
                IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
                IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);
                var token = encoder.Encode(payload, cKey);
                return token;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return string.Empty;
            }
        }

        /// <summary>
        /// 用JWT套件解碼Token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static Token DecodeToken(string token)
        {
            try
            {
                IJsonSerializer serializer = new JsonNetSerializer();
                var provider = new UtcDateTimeProvider();
                IJwtValidator validator = new JwtValidator(serializer, provider);
                IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
                IJwtAlgorithm algorithm = new HMACSHA256Algorithm(); // symmetric
                IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder, algorithm);

                var json = decoder.Decode(token, cKey, verify: true);
                var payload = serializer.Deserialize<Token>(json);
                return payload;
            }
            catch (TokenExpiredException ex)
            {
                Log.Error(new Exception("Token has expired", ex));
            }
            catch (SignatureVerificationException ex)
            {
                Log.Error(new Exception("Token has invalid signature", ex));
            }
            return null;
        }

        public class Token
        {
            public long lastCreateTokenAt { get; set; }
            
            public string salt { get; set; }

            public long uid { get; set; }
        }
    }
}
