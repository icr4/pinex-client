using System;
using System.Collections.Generic;
using Models;
using Serializers.Payload;

namespace Packets
{
    public class Builder
    {
        public static Dictionary<string, string> SocketLogin(string token)
        {
            return new Dictionary<string, string> {
                { "token", token }
            };
        }

        public static Dictionary<string, object> StartMatchmaking(string poolId)
        {
            return new Dictionary<string, object> {
                { "pool_id", poolId }
            };
        }

        public static Dictionary<string, object> StartMatchmaking(Friend friend)
        {
            return new Dictionary<string, object> {
                { "user_id", friend.id }
            };
        }

        public static Dictionary<string, object> DrawCard(Nullable<int> index)
        {
            return new Dictionary<string, object> {
                { "grounded_index", index }
            };
        }

        public static Dictionary<string, object> DropCard(Card card)
        {
            return new Dictionary<string, object> {
                { "card", card.serialized() }
            };
        }

        public static Dictionary<string, object> PutSet(List<Card> cards)
        {
            return new Dictionary<string, object> {
                { "cards", CardList(cards) }
            };
        }

        public static Dictionary<string, object> PutCard(List<Card> cards, string setId, Position position)
        {
            return new Dictionary<string, object> {
                { "cards", CardList(cards)},
                { "set_id", setId },
                { "position", position.ToString().ToLower() }
            };
        }

        public static Dictionary<string, object> Chat(string message)
        {
            return new Dictionary<string, object> {
                { "message", message }
            };
        }

        private static List<CardPayload> CardList(List<Card> cards)
        {
            return cards.ConvertAll(card => new CardPayload
            {
                number = card.number,
                seed = card.seed
            });
        }
    }
}