﻿using System;

using Flurl.Http;

using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Draft.Responses;

using Flurl;

namespace Draft.Requests
{
    internal class UpsertQueueRequest : BaseRequest, IUpsertKeyRequest, ICreateDirectoryRequest, IUpdateDirectoryRequest, IQueueRequest
    {

        public UpsertQueueRequest(Url endpointUrl, string path)
            : base(endpointUrl, path) {}

        public bool? Existing { get; private set; }

        public bool IsDirectory { get; set; }

        public bool IsQueue { get; set; }

        public long? Ttl { get; private set; }

        public string Value { get; private set; }

        ICreateDirectoryRequest ICreateDirectoryRequest.WithTimeToLive(long? seconds)
        {
            WithTimeToLive(seconds);
            return this;
        }

        IQueueRequest IQueueRequest.WithTimeToLive(long? seconds)
        {
            WithTimeToLive(seconds);
            return this;
        }

        IQueueRequest IQueueRequest.WithValue(string value)
        {
            WithValue(value);
            return this;
        }

        IUpdateDirectoryRequest IUpdateDirectoryRequest.WithTimeToLive(long? seconds)
        {
            WithExisting();
            WithTimeToLive(seconds);
            return this;
        }

        public async Task<IKeyEvent> Execute()
        {
            var values = new ListDictionary
            {
                {
                    // Key
                    IsDirectory ? EtcdConstants.Parameter_Directory : EtcdConstants.Parameter_Value,
                    // Value
                    IsDirectory ? EtcdConstants.Parameter_True : Value
                }
            };

            if (Existing.HasValue && Existing.Value)
            {
                values.Add(EtcdConstants.Parameter_PrevExist, EtcdConstants.Parameter_True);
            }

            if (Ttl.HasValue)
            {
                values.Add(EtcdConstants.Parameter_Ttl, Ttl.Value);
            }

            return await TargetUrl
                .Conditionally(IsQueue, values, (x, v) => x.PostUrlEncodedAsync(v), (x, v) => x.PutUrlEncodedAsync(v))
                .ReceiveEtcdResponse<KeyEvent>();
        }

        public TaskAwaiter<IKeyEvent> GetAwaiter()
        {
            return Execute().GetAwaiter();
        }

        public IUpsertKeyRequest WithExisting(bool existing = true)
        {
            Existing = existing;
            return this;
        }

        public IUpsertKeyRequest WithTimeToLive(long? seconds)
        {
            Ttl = seconds;
            return this;
        }

        public IUpsertKeyRequest WithValue(string value)
        {
            Value = value;
            return this;
        }

    }
}
