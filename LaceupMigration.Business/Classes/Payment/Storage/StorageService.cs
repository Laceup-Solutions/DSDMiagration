using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace LaceupMigration
{
    public class StorageService
    {
        List<PaymentRequest> Cards = new List<PaymentRequest>();
        Client client;
        public async Task AddCardAsync(PaymentRequest paymentRequest)
        {
            var storedData = await SecureStorage.GetAsync("CardList");
            List<PaymentRequest> cards = string.IsNullOrEmpty(storedData)
                ? new List<PaymentRequest>() : JsonConvert.DeserializeObject<List<PaymentRequest>>(storedData);
            cards.Add(paymentRequest);

            if (cards.Count > 0)
            {
                Console.WriteLine("hay elementos en la lsita");
            }

            var jsonString = JsonConvert.SerializeObject(cards);

            await SecureStorage.SetAsync("CardList", jsonString);
            await GetAllCardsAsync();
        }

        public async Task AddAuthorizedTokenAsync(PaymentAtutorizeToken token)
        {
            var storedDataToken = await SecureStorage.GetAsync("CardListToken");
            List<PaymentAtutorizeToken> cardsToken = string.IsNullOrEmpty(storedDataToken)
                ? new List<PaymentAtutorizeToken>() : JsonConvert.DeserializeObject<List<PaymentAtutorizeToken>>(storedDataToken);
        }

        public async Task RemoveCardAsync(PaymentRequest cardToRemove)
        {
            string storedData = await SecureStorage.GetAsync("CardList");
            List<PaymentRequest> cards = JsonConvert.DeserializeObject<List<PaymentRequest>>(storedData);
            var cardInStorage = cards.FirstOrDefault(card => card.AccountNumber == cardToRemove.AccountNumber);

            if (cardInStorage != null)
            {
                cards.Remove(cardInStorage);
                await SecureStorage.SetAsync("CardList", JsonConvert.SerializeObject(cards));
            }
            else
            {
                Console.WriteLine("La tarjeta no se encontró en el almacenamiento.");
            }
        }

        public async Task<List<PaymentRequest>> GetAllCardsAsync()
        {
            var storedData = await SecureStorage.GetAsync("CardList");
            if (string.IsNullOrEmpty(storedData))
            {
                return new List<PaymentRequest>();

            }
            return JsonConvert.DeserializeObject<List<PaymentRequest>>(storedData);
        }

        public async Task<List<PaymentAtutorizeToken>> GetAllCardsTokenAsync()
        {
            var storedDataToken = await SecureStorage.GetAsync("OperationList");
            if (string.IsNullOrEmpty(storedDataToken))
            {
                return new List<PaymentAtutorizeToken>();
            }
            return JsonConvert.DeserializeObject<List<PaymentAtutorizeToken>>(storedDataToken);

        }

        public async Task<PaymentRequest> GetCardAsync(string accountToken)
        {
            var storedData = await SecureStorage.GetAsync("CardList");

            if (string.IsNullOrEmpty(storedData))
            {
                return null;
            }

            var cards = JsonConvert.DeserializeObject<List<PaymentRequest>>(storedData);
            var card = cards.FirstOrDefault(c => c.AccountToken == accountToken); // Cambié c.AccountNumber a c.AccountToken
            return card;
        }


        public async Task<bool> ExistsStorageAsync(PaymentRequest cardToRemove)
        {
            string storedData = await SecureStorage.GetAsync("CardList");
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