using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Web;

namespace NoticeSuite.Extensions
{
    public static class WcfExtensions
    {
        public static void Using<T>(this T client, Action<T> work)
            where T : ICommunicationObject
        {
            try
            {
                work(client);
                client.Close();
            }
            catch (CommunicationException e)
            {
                client.Abort();
            }
            catch (TimeoutException e)
            {
                client.Abort();
            }
            catch (Exception e)
            {
                client.Abort();
                throw;
            }
        }
    }
}