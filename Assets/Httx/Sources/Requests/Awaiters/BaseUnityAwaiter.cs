// Copyright (c) 2020 Sergey Ivonchik
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE
// OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using UnityEngine;
using UnityEngine.Networking;

namespace Httx.Requests.Awaiters {
  public abstract class BaseUnityAwaiter<TResult> : IAwaiter<TResult> {
    private readonly IRequest inputRequest;
    private UnityWebRequestAsyncOperation operation;
    private Action<AsyncOperation> continuationAction;
    private bool isAwaken;

    public BaseUnityAwaiter(IRequest request) {
      inputRequest = request;
    }

    public void OnCompleted(Action continuation) {
      continuationAction = asyncOperation => continuation();
      operation.completed += continuationAction;
    }

    public bool IsCompleted {
      get {
        if (isAwaken) {
          return operation.isDone;
        }

        operation = Awake(inputRequest);
        isAwaken = true;

        return operation.isDone;
      }
    }

    public TResult GetResult() {
      // TODO: Exceptions
      if (!string.IsNullOrEmpty(operation.webRequest.error)) {
        throw new Exception(operation.webRequest.error);
      }

      if (null == continuationAction) {
        return OnResult(inputRequest, operation);
      }

      operation.completed -= continuationAction;
      continuationAction = null;

      try {
        return OnResult(inputRequest, operation);
      } finally {
        operation = null;
      }
    }

    public abstract UnityWebRequestAsyncOperation Awake(IRequest request);
    public abstract TResult OnResult(IRequest request, UnityWebRequestAsyncOperation operation);
  }
}