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

using System.Collections.Generic;
using System.Linq;
using Httx.Requests.Attributes;
using Httx.Requests.Awaiters;
using Httx.Requests.Mappers;

namespace Httx.Requests.Types {
  [Awaiter(typeof(UnityWebRequestAwaiter<>))]
  [Mapper(typeof(NopMapper<,>))]
  public class Bytes : BaseRequest {
    public Bytes(string url, IEnumerable<byte> body = null) : base(null) {
      Url = url;
      Body = body;
    }

    public override string Url { get; }
    public override IEnumerable<byte> Body { get; }

    public override IEnumerable<KeyValuePair<string, object>> Headers {
      get {
        var headers = new Dictionary<string, object> {
          { "Accept", "application/octet-stream" }
        };

        if (null != Body && 0 != Body.Count()) {
          headers.Add("Content-Type", "application/octet-stream");
        }

        return headers;
      }
    }
  }
}
