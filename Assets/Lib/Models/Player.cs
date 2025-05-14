using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using Constants;

namespace Models
{
    public class Player
    {
        [JsonProperty("id")]
        public string id { get; set; }
        [JsonProperty("username")]
        public string username { get; set; }
        [JsonProperty("avatar")]
        public string avatar { get; set; }

        [JsonProperty("cards")]
        public List<Card> cards { get; set; }
        [JsonProperty("team")]
        public int team;
        [JsonProperty("level")]
        public int level;
        [JsonProperty("cards_count")]
        public int cards_count;
        public GameObject avatarObj;

        private DateTime lastTurnAt;

        public void SetAvatar(GameObject avatar)
        {
            this.avatarObj = avatar;
            this.avatarObj.SetActive(true);
        }

        public void ToggleTurn(bool enabled)
        {
            SoundController.instance.clockAudioSource.Stop();

            if (enabled)
            {
                this.lastTurnAt = DateTime.UtcNow;
            }

            if (this.avatarObj == null)
            {
                InputController.instance.drawBtn.transform.Find("TurnCooldown").GetComponent<Image>().fillAmount = enabled ? 1 : 0;
            }
            else
            {
                this.avatarObj.transform.Find("Profile/Turn").GetComponent<Image>().fillAmount = enabled ? 1 : 0;
            }
        }

        public IEnumerator ReduceTurnTimer()
        {
            while (true)
            {
                float percentage = (float)(DateTime.UtcNow - this.lastTurnAt).TotalMilliseconds / ServerConstants.TURN_DURATION;

                if (!SoundController.instance.clockAudioSource.isPlaying && percentage >= 0.85)
                {
                    SoundController.instance.clockAudioSource.Play();
                }

                if (this.avatarObj != null)
                {
                    this.avatarObj.transform.Find("Profile/Turn").GetComponent<Image>().fillAmount = percentage;
                }
                else
                {
                    InputController.instance.drawBtn.transform.Find("TurnCooldown").GetComponent<Image>().fillAmount = percentage;
                }

                if (percentage >= 1)
                {
                    SoundController.instance.clockAudioSource.Stop();
                    yield break;
                }
                else yield return null;
            }
        }

        public void UpdateCardsCount(int count)
        {
            this.cards_count += count;
            if (this.cards_count < 0) this.cards_count = 0;

            this.avatarObj.transform.Find("PlayerDeck/Count").GetComponent<Text>().text = $"{this.cards_count}";
        }
    }
}