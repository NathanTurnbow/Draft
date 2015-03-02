﻿using System;
using System.Linq;

using Draft.Responses;

namespace Draft.Requests
{
    /// <summary>
    ///     A request to watch for changes on a key or key's children.
    /// </summary>
    public interface IWatchRequest : IObservable<IKeyEvent>
    {

        /// <summary>
        ///     The modified index of the key to start watching from.
        /// </summary>
        IWatchRequest WithModifiedIndex(int? index = null);

        /// <summary>
        ///     When <c>true</c>, also watch for change's in this key's children.
        /// </summary>
        IWatchRequest WithRecursive(bool isRecursive = true);

    }
}
