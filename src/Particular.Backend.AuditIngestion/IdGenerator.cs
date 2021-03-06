namespace ServiceControl.MessageTypes
{
    using System;
    using System.Linq;
    using Particular.Operations.Ingestion.Api;
    using ServiceControl.Infrastructure;

    public class IdGenerator
    {
        public string ParseId(HeaderCollection headers)
        {
            string messageId;
            if (!headers.TryGet("NServiceBus.MessageId", out messageId))
            {
                throw new Exception("Cannot generate a unique id for a message that does contain a message id.");
            }
            return messageId;
        }

        public string GenerateUniqueId(HeaderCollection headers)
        {
            var messageId = ParseId(headers);
            return DeterministicGuid.MakeId(messageId, ParseProcessingEndpointName(headers, messageId)).ToString();
        }

        static string ParseProcessingEndpointName(HeaderCollection headers, string messageId)
        {
            string endpoint;

            if (headers.TryGet("NServiceBus.ProcessingEndpoint", out endpoint))
            {
                return endpoint;
            }

            string replyToAddress;
            if (headers.TryGet("NServiceBus.ReplyToAddress", out replyToAddress))
            {
                return replyToAddress.Split('@').First();
            }

            // If the ReplyToAddress is null, then the message came from a send-only endpoint. 
            // This message could be a failed message.  
            if (headers.TryGet("NServiceBus.FailedQ", out endpoint))
            {
                return endpoint;
            }

            string messageTypes;
            if (headers.TryGet("NServiceBus.EnclosedMessageTypes", out messageTypes))
            {
                throw new Exception(string.Format("No processing endpoint could be determined for message ({0}) with EnclosedMessageTypes ({1})", messageId, messageTypes));
            }

            throw new Exception(string.Format("No processing endpoint could be determined for message ({0})", messageId));
        }

    }
}