using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using Constants;

namespace Models
{
    public class Card
    {
        public enum Type { Deck, Ground, Set, Animate };

        [JsonProperty("number")]
        public int number { get; set; }
        [JsonProperty("seed")]
        public int seed { get; set; }
        public GameObject obj;
        public Boolean handSelected = false;
        public Type type;
        public string setId;
        public Vector2 originalPos;

        public Card(int seed, int number)
        {
            this.seed = seed;
            this.number = number;
        }

        public void ToggleSelected()
        {
            this.handSelected = !this.handSelected;

            if (this.handSelected)
            {
                this.obj.transform.position = new Vector2(this.obj.transform.position.x, this.obj.transform.position.y + 10);
                InterfaceController.instance.handSelected.Add(this);
            }
            else
            {
                this.obj.transform.position = new Vector2(this.obj.transform.position.x, this.obj.transform.position.y - 10);
                InterfaceController.instance.handSelected.Remove(this);
            }
        }

        public bool IsJolly()
        {
            return new List<int> { 5, 6 }.Contains(this.seed) && this.number == 15;
        }

        public string GetChatValue()
        {
            string seed = ClientConstants.CARD_UTF_SEEDS[this.seed - 1];
            string color = ClientConstants.CARD_UTF_COLORS[this.seed - 1];
            string number = ClientConstants.CARD_UTF_NUMBERS[this.number - 2];

            return $"<color={color}>{number}{seed}</color>";
        }

        public Serializers.Payload.CardPayload serialized()
        {
            return new Serializers.Payload.CardPayload
            {
                number = this.number,
                seed = this.seed
            };
        }
    }
}