using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LaceupMigration
{
    public class StorageService
    {
        List<PaymentRequest> Cards = new List<PaymentRequest>();
        Client client;
        public void AddCard(PaymentRequest paymentRequest)
        {
            var storedData = Xamarin.Essentials.SecureStorage.GetAsync("CardList").Result;
            List<PaymentRequest> cards = string.IsNullOrEmpty(storedData)
                ? new List<PaymentRequest>() : JsonConvert.DeserializeObject<List<PaymentRequest>>(storedData);
            cards.Add(paymentRequest);

            if (cards.Count > 0)
            {
                Console.WriteLine("hay elementos en la lsita");
            }

            var jsonString = JsonConvert.SerializeObject(cards);

            Xamarin.Essentials.SecureStorage.SetAsync("CardList", jsonString);
            GetAllCards();
        }

        public void AddAuthorizedToken(PaymentAtutorizeToken token)
        {
            var storedDataToken = Xamarin.Essentials.SecureStorage.GetAsync("CardListToken").Result;
            List<PaymentAtutorizeToken> cardsToken = string.IsNullOrEmpty(storedDataToken)
                ? new List<PaymentAtutorizeToken>() : JsonConvert.DeserializeObject<List<PaymentAtutorizeToken>>(storedDataToken);
        }

        public void RemoveCard(PaymentRequest cardToRemove)
        {
            string storedData = Xamarin.Essentials.SecureStorage.GetAsync("CardList").Result;
            List<PaymentRequest> cards = JsonConvert.DeserializeObject<List<PaymentRequest>>(storedData);
            var cardInStorage = cards.FirstOrDefault(card => card.AccountNumber == cardToRemove.AccountNumber);

            if (cardInStorage != null)
            {
                cards.Remove(cardInStorage);
                Xamarin.Essentials.SecureStorage.SetAsync("CardList", JsonConvert.SerializeObject(cards)).Wait();
            }
            else
            {
                Console.WriteLine("La tarjeta no se encontró en el almacenamiento.");
            }
        }

        public List<PaymentRequest> GetAllCards()
        {
            var storedData = Xamarin.Essentials.SecureStorage.GetAsync("CardList").Result;
            if (string.IsNullOrEmpty(storedData))
            {
                return new List<PaymentRequest>();

            }
            return JsonConvert.DeserializeObject<List<PaymentRequest>>(storedData);
        }

        public List<PaymentAtutorizeToken> GetAllCardsToken()
        {
            var storedDataToken = Xamarin.Essentials.SecureStorage.GetAsync("OperationList").Result;
            if (string.IsNullOrEmpty(storedDataToken))
            {
                return new List<PaymentAtutorizeToken>();
            }
            return JsonConvert.DeserializeObject<List<PaymentAtutorizeToken>>(storedDataToken);

        }

        public PaymentRequest GetCard(string accountToken)
        {
            var storedData = Xamarin.Essentials.SecureStorage.GetAsync("CardList").Result;

            if (string.IsNullOrEmpty(storedData))
            {
                return null;
            }

            var cards = JsonConvert.DeserializeObject<List<PaymentRequest>>(storedData);
            var card = cards.FirstOrDefault(c => c.AccountToken == accountToken); // Cambié c.AccountNumber a c.AccountToken
            return card;
        }


        public bool existsStorage(PaymentRequest cardToRemove)
        {
            string storedData = Xamarin.Essentials.SecureStorage.GetAsync("CardList").Result;
            List<PaymentRequest> cards = JsonConvert.DeserializeObject<List<PaymentRequest>>(storedData);
            var cardInStorage = cards.FirstOrDefault(card => card.AccountNumber == cardToRemove.AccountNumber);

            if (cardInStorage != null)
            {
                return true;
            }
            else
            {
                Console.WriteLine("La tarjeta no se encontró en el almacenamiento.");
                return false;
            }
        }

    }
}