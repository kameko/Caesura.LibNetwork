
namespace Caesura.LibNetwork.Http
{
    using System;
    using System.Text;
    
    // https://en.wikipedia.org/wiki/List_of_HTTP_status_codes 
    
    // TODO: replace this Enum for some kind of static Key/Value class.
    // This way, both the consumer can add custom status codes, and each
    // status code can have a custom formatted message instead of having to
    // use the static methods below.
    
    public enum HttpStatusCode
    {
        Unkown                          = 0,
        //
        // Informational 1xx
        //
        Continue                        = 100,
        SwitchingProtocols              = 101,
        Processing                      = 102,
        Checkpoint                      = 103,
 
        //
        // Successful 2xx
        //
        OK                              = 200,
        Created                         = 201,
        Accepted                        = 202,
        NonAuthoritativeInformation     = 203,
        NoContent                       = 204,
        ResetContent                    = 205,
        PartialContent                  = 206,
        AlreadyReported                 = 208,
 
        //
        // Redirection 3xx
        //
        MultipleChoices                 = 300,
        Ambiguous                       = 300,
        MovedPermanently                = 301,
        Moved                           = 301,
        Found                           = 302,
        Redirect                        = 302,
        SeeOther                        = 303,
        RedirectMethod                  = 303,
        NotModified                     = 304,
        UseProxy                        = 305,
        Unused                          = 306,
        TemporaryRedirect               = 307,
        RedirectKeepVerb                = 307,
        PermanentRedirect               = 308,
 
        //
        // Client Error 4xx
        //
        BadRequest                      = 400,
        Unauthorized                    = 401,
        PaymentRequired                 = 402,
        Forbidden                       = 403,
        NotFound                        = 404,
        MethodNotAllowed                = 405,
        NotAcceptable                   = 406,
        ProxyAuthenticationRequired     = 407,
        RequestTimeout                  = 408,
        Conflict                        = 409,
        Gone                            = 410,
        LengthRequired                  = 411,
        PreconditionFailed              = 412,
        RequestEntityTooLarge           = 413,
        RequestUriTooLong               = 414,
        UnsupportedMediaType            = 415,
        RequestedRangeNotSatisfiable    = 416,
        ExpectationFailed               = 417,
        ImATeapot                       = 418,
        MisdirectedRequest              = 421,
        UnprocessableEntity             = 422,
        Locked                          = 423,
        FailedDependency                = 424,
        TooEarly                        = 425,
        UpgradeRequired                 = 426,
        PreconditionRequired            = 428,
        TooManyRequests                 = 429,
        RequestHeaderFieldsTooLarge     = 431,
        UnavailableForLegalReasons      = 451,
 
        //
        // Server Error 5xx
        //
        InternalServerError             = 500,
        NotImplemented                  = 501,
        BadGateway                      = 502,
        ServiceUnavailable              = 503,
        GatewayTimeout                  = 504,
        HttpVersionNotSupported         = 505,
        VariantAlsoNegotiates           = 506,
        InsufficientStorage             = 507,
        LoopDetected                    = 508,
        NotExtended                     = 510,
        SSLHandshakeFailed              = 525,
    }
    
    public static class HttpStatusCodeUtils
    {
        public static bool CheckStatusCodeInRange(HttpStatusCode status_code, int begin, int end)
        {
            var code = (int)status_code;
            return code >= begin && code < end;
        }
        
        public static bool IsInformationalStatusCode(HttpStatusCode code) => CheckStatusCodeInRange(code, 100, 200);
        public static bool IsSuccessStatusCode(HttpStatusCode code)       => CheckStatusCodeInRange(code, 200, 300);
        public static bool IsRedirectionStatusCode(HttpStatusCode code)   => CheckStatusCodeInRange(code, 300, 400);
        public static bool IsClientErrorStatusCode(HttpStatusCode code)   => CheckStatusCodeInRange(code, 400, 500);
        public static bool IsServerErrorStatusCode(HttpStatusCode code)   => CheckStatusCodeInRange(code, 500, 600);
        
        public static bool ConvertFromNumber(int number, out HttpStatusCode code)
        {
            if (Enum.IsDefined(typeof(HttpStatusCode), number))
            {
                code = (HttpStatusCode)number;
                return true;
            }
            else
            {            
                code = HttpStatusCode.Unkown;
                return false;
            }
        }
        
        public static int ConvertToNumber(HttpStatusCode code)
        {
            return (int)code;
        }
        
        public static string ConvertToNumberString(HttpStatusCode code)
        {
            return ConvertToNumber(code).ToString();
        }
        
        public static string ConvertToFormattedString(HttpStatusCode code) => ConvertToFormattedString(code, true);
        
        public static string ConvertToFormattedString(HttpStatusCode code, bool capitalize_each_word)
        {
            return code switch
            {
                HttpStatusCode.OK                 => "OK",
                HttpStatusCode.ImATeapot          => "I'm a teapot", // Intentionally doesn't check "capitalize_each_word".
                HttpStatusCode.SSLHandshakeFailed => capitalize_each_word ? "SSL Handshake Failed" : "SSL handshake failed",
                _ => ConvertAndFormat(code, capitalize_each_word)
            };
        }
        
        private static string ConvertAndFormat(HttpStatusCode code, bool capitalize_each_word)
        {
            var sb  = new StringBuilder();
            var str = code.ToString();
            
            // Append the first character, then get
            // the substring, so we don't add a space
            // before the first word.
            sb.Append(str[0]);
            foreach (var c in str.Substring(1))
            {
                if (char.IsUpper(c))
                {
                    sb.Append(' ');
                }
                
                if (char.IsUpper(c))
                {
                    if (capitalize_each_word)
                    {
                        sb.Append(c);
                    }
                    else
                    {
                        sb.Append(char.ToLower(c));
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }
}
