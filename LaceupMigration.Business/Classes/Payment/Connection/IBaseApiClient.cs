





using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaceupMigration
{
    public interface IBaseApiClient
    {
        Task<PaymentResponse> Authorize(PaymentRequest paymentRequest);

        Task<PaymentResponse> AuthorizeToken(PaymentAtutorizeToken paymentAtutorizeToken);
    }
}