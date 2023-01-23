using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace signalRTest.Hubs
{
    public class DiaryMessagingHub : Hub
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

        public async Task<SalesEntryMessage> EdtMessage([FromBody] UpdateDiaryMessageModel message)
        {
            await Task.Delay(5000);
            var res = new SalesEntryMessage
            {
                SalesOrderId = 45504,
                SalesServiceLineId = null,
                SalesOrderLineId = null,
                CosmosDBId = new Guid(message.MessageId),
                ObjectID = "45504",
                LastUpdatedAt = DateTime.Now,
                UserADId = "fa89a99c-cf31-430b-9b03-467e4d870f67",
                Message = new MessageEnvelope
                {
                    Text = $"<p>{message.MessageText}</p>",
                    Mentions = new List<DiaryMessageMention>()
                },
                RemovedAt = null,
                User = "Ivanna Kitsera",
                OccuredAt = DateTime.Parse("2023-01-23T04:46:09.6168956Z"),
            };
            await Clients.AllExcept(Context.ConnectionId).SendAsync("EditedDiaryMessageReceived", res);
            return res;
        }

        public async Task<string> DeleteMessage([FromBody] DeleteMessageModel something)
        {
            await Task.Delay(5000);
            await Clients.AllExcept(Context.ConnectionId).SendAsync("DeletedDiaryMessageReceived", something.MessageId);
            return something.MessageId;
        }


        public async Task AddedHistoricalRecord(SalesEntryHistoricalRecord record)
        {
            await Task.Delay(1000);
            var res = new SalesEntryHistoricalRecord
            {
                SalesOrderId = 45504,
                SalesOrderLineId = null,
                SalesServiceLineId = null,
                Action = 0,
                ObjectID = "45504",
                Diff = JToken.FromObject(
                     new List<string>
                     {
                        "DVD",
                        "SSD"
                     }
                ),
                EventData = null,
                EntityType = "SalesOrder",
                User = "Ivanna Kitsera",
                OccuredAt = DateTime.Now
            };

            await Clients.All.SendAsync("NewDiaryItemReceived", res);
        }


        public enum AuditActionV2
        {
            Create,
            Update,
            Delete,
            Event
        }

        public enum EventMetadataType
        {
            TripletexOrderId,
            ControllingStatusChanged

        }
        public interface IDiaryHistoricalRecord : IDiaryItem
        {
            public AuditActionV2 Action { get; set; }
            public string ObjectID { get; }
            public JToken? Diff { get; set; }
        }

        public abstract class DiaryHistoricalRecord : DiaryItem, IDiaryHistoricalRecord
        {
            public AuditActionV2 Action { get; set; }
            public string ObjectID { get; set; }
            public JToken? Diff { get; set; }
            public DiaryHistroicalRecordActionMetadata? EventData { get; set; }
            public override DiaryItemTypes DiaryItemType => DiaryItemTypes.HistoricalRecord;
        }

        public class DiaryHistroicalRecordActionMetadata
        {
            public string Message { get; set; }
            public string Data { get; set; }
            public EventMetadataType Type { get; set; }
        }

        public class SalesEntryHistoricalRecord : DiaryHistoricalRecord
        {
            public int SalesOrderId { get; set; }

            public int? SalesOrderLineId { get; set; }

            public int? SalesServiceLineId { get; set; }
        }
    }


    public class DeleteMessageModel
    {
        [Required]
        public string ObjectType { get; set; }
        [Required]
        public string MessageId { get; set; }
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


    public class UpdateDiaryMessageModel
    {
        [Required]
        public string MessageId { get; set; }
        [Required]
        public string ObjectType { get; set; }
        [Required]
        [MaxLength(5000)]
        public string MessageText { get; set; }
    }

    public class SalesEntryMessage : DiaryMessage
    {

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
