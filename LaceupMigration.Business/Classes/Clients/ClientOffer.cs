using System.Collections.Generic;
using System.Linq;

namespace LaceupMigration
{
    public class ClientsOffer
    {
        public int ClientId { get; set; }

        public int OfferId { get; set; }

        public static void Clear(int count)
        {
            offers.Clear();
        }

        static Dictionary<int, List<ClientsOffer>> offers = new Dictionary<int, List<ClientsOffer>>();

        public static void AddClientsOffer(ClientsOffer offer)
        {
            if (!offers.ContainsKey(offer.ClientId))
                offers.Add(offer.ClientId, new List<ClientsOffer>());

            offers[offer.ClientId].Add(offer);
        }

        internal static bool IsOfferVisibleToClient(Offer offer, Client client)
        {
            if (!offers.ContainsKey(client.ClientId))
                return false;

            var co = offers[client.ClientId].FirstOrDefault(x => x.OfferId == offer.OfferId);

            return co != null;
        }
    }
}

