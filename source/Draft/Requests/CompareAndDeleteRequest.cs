﻿using System;

using Flurl.Http;

using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Draft.Endpoints;
using Draft.Responses;

namespace Draft.Requests
{
    internal class CompareAndDeleteRequest : BaseRequest, ICompareAndDeleteRequest, ICompareAndDeleteByIndexRequest, ICompareAndDeleteByValueRequest
    {

        public CompareAndDeleteRequest(IEtcdClient etcdClient, EndpointPool endpointPool, params string[] pathParts)
            : base(etcdClient, endpointPool, pathParts) {}

        public long ExpectedIndex { get; private set; }

        public string ExpectedValue { get; private set; }

        Task<IKeyEvent> ICompareAndDeleteByIndexRequest.Execute()
        {
            return Execute(false);
        }

        TaskAwaiter<IKeyEvent> ICompareAndDeleteByIndexRequest.GetAwaiter()
        {
            return GetAwaiter(false);
        }

        Task<IKeyEvent> ICompareAndDeleteByValueRequest.Execute()
        {
            return Execute(true);
        }

        TaskAwaiter<IKeyEvent> ICompareAndDeleteByValueRequest.GetAwaiter()
        {
            return GetAwaiter(true);
        }

        public ICompareAndDeleteByIndexRequest WithExpectedIndex(long modifiedIndex)
        {
            ExpectedIndex = modifiedIndex;
            return this;
        }

        public ICompareAndDeleteByValueRequest WithExpectedValue(string value)
        {
            ExpectedValue = value;
            return this;
        }

        private async Task<IKeyEvent> Execute(bool isByValue)
        {
            try
            {
                return await TargetUrl
                    .Conditionally(isByValue, x => x.SetQueryParam(Constants.Etcd.Parameter_PrevValue, ExpectedValue))
                    .Conditionally(!isByValue, x => x.SetQueryParam(Constants.Etcd.Parameter_PrevIndex, ExpectedIndex))
                    .DeleteAsync()
                    .ReceiveEtcdResponse<KeyEvent>(EtcdClient);
            }
            catch (FlurlHttpException e)
            {
                throw await e.ProcessException();
            }
        }

        private TaskAwaiter<IKeyEvent> GetAwaiter(bool isByValue)
        {
            return Execute(isByValue).GetAwaiter();
        }

    }
}
