using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;
using System.Threading;
using System;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace signalRTest.Hubs
{
    public class DiaryMessagingHub: Hub
    {
        public async Task<SalesEntryMessage> SendMessage([FromBody] AddDiaryMessageModel<int> newMssage)
        {
            await Task.Delay(5000);
            var res = new SalesEntryMessage()
            {
                SalesOrderId = newMssage.ObjectId,
                SalesOrderLineId = null,
                SalesServiceLineId = null,
                CosmosDBId = new Guid("c2786794-06fa-4cf8-953d-b6bb56b5cf67"),
                ObjectID = "45489",
                LastUpdatedAt = DateTime.Now,
                UserADId = "fa89a99c-cf31-430b-9b03-467e4d870f67",
                Message = new MessageEnvelope
                {
                    Text = $"<p>{newMssage.MessageText}</p>",
                    Mentions = new List<DiaryMessageMention>()
                },
                RemovedAt = null,
                User = "Ivanna Kitsera",
                OccuredAt = DateTime.Now

            };
            await Clients.AllExcept(Context.ConnectionId).SendAsync("NewDiaryItemReceived", res);
            return res;
        }

        public async Task EdtMessage(string something)
        {
            await Task.Delay(5000);
            await Clients.AllExcept(Context.ConnectionId).SendAsync("EditedDiaryMessageReceived", something);
        }

        public async Task DeleteMessage(string something)
        {
            await Task.Delay(5000);
            await Clients.AllExcept(Context.ConnectionId).SendAsync("DeletedDiaryMessageReceived", something);
        }


        public async Task ReadAllMessages()
        {
            await Task.Delay(3000);
            await Clients.Clients(Context.ConnectionId).SendAsync("ReadAllMessages");
        }

   
    }

    public class AddDiaryMessageModel<T>
    {

        [Required]
        public T ObjectId { get; set; }

        [Required]
        public string ObjectType { get; set; }

        [Required]
        [MaxLength(5000)]
        public string MessageText { get; set; }
    }

    public class SalesEntryMessage : DiaryMessage { 

        public int SalesOrderId { get; set; }

        public int? SalesOrderLineId { get; set; }

        public int? SalesServiceLineId { get; set; }
    }

    public abstract class DiaryMessage : DiaryItem, IDiaryMessage
    {
        [JsonProperty(PropertyName = "id")]
        public Guid CosmosDBId { get; set; }

        public string ObjectID { get; set; }

        public DateTime LastUpdatedAt { get; set; }

        public string UserADId { get; set; }

        public MessageEnvelope Message { get; set; }

        public DateTime? RemovedAt { get; set; }

        public override DiaryItemTypes DiaryItemType => DiaryItemTypes.Message;

        public bool IsRemoved => RemovedAt.HasValue;

        public bool IsEdited => LastUpdatedAt != OccuredAt;
    }

    public abstract class DiaryItem : IDiaryItem
    {
        public string EntityType { get; set; }
        public string User { get; set; }
        public DateTime OccuredAt { get; set; }
        public abstract DiaryItemTypes DiaryItemType { get; }
    }

    public interface IDiaryItem
    {
        public string EntityType { get; set; }
        public string User { get; set; }
        public DateTime OccuredAt { get; set; }
        public DiaryItemTypes DiaryItemType { get; }
    }

    public interface IDiaryMessage : IDiaryItem
    {
        public DateTime LastUpdatedAt { get; set; }

        public string UserADId { get; set; }

        public MessageEnvelope Message { get; set; }
    }

    public enum DiaryItemTypes
    {
        HistoricalRecord = 1,
        Message = 2
    }

    public class MessageEnvelope
    {
        public string Text { get; set; }

        public IEnumerable<DiaryMessageMention> Mentions { get; set; }
    }

    public class DiaryMessageMention
    {
        [JsonProperty(PropertyName = "azure_ad_id")]
        public string EcohzContactAzureAdId { get; set; }

        [JsonProperty(PropertyName = "ecohz_contact_id")]
        public long? EcohzContactId { get; set; }
    }
}
