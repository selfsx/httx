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
using System.Collections.Generic;
using System.Linq;
using Httx.Utils;

namespace Httx.Cache {
  internal class Item<T> {
    public Item(string key, T value, int ttl, long createdAt) {
      Key = key;
      Value = value;
      Ttl = ttl;
      CreatedAt = createdAt;
    }

    public Item(string key, T value, int ttl) : this(key, value, ttl, Time.UnixTime()) { }

    public string Key { get; }
    public T Value { get; }
    public int Ttl { get; }
    public long CreatedAt { get; }
    public bool Expired => Time.Since(CreatedAt) >= Ttl;
  }

  public class MemoryCache<T> {
    private readonly object selfLock = new object();
    private readonly int size;
    private readonly int defaultTtl;
    private readonly int collectFrequency;
    private int collectAfter;

    private Dictionary<string, LinkedListNode<Item<T>>> cacheImpl =
      new Dictionary<string, LinkedListNode<Item<T>>>();

    private LinkedList<Item<T>> lruImpl =
      new LinkedList<Item<T>>();

    public MemoryCache(int size, int defaultTtl = int.MaxValue, int collectFrequency = 0) {
      this.size = size;
      this.defaultTtl = defaultTtl;
      this.collectFrequency = collectFrequency;

      var isExpirationEnabled = int.MaxValue != defaultTtl && 0 != collectFrequency;
      collectAfter = isExpirationEnabled ? collectFrequency : -1;
    }

    public void Put(string key, T value, int ttl) {
      lock (selfLock) {
        TryCollect();

        if (cacheImpl.Count >= size) {
          RemoveFirst();
        }

        var itemTtl = 0 != collectFrequency ? ttl : int.MaxValue;
        var node = new LinkedListNode<Item<T>>(new Item<T>(key, value, itemTtl));

        lruImpl.AddLast(node);
        cacheImpl[key] = node;
      }
    }

    public void Put(string key, T value) {
      Put(key, value, defaultTtl);
    }

    public T Get(string key, Func<T> defaultFunc = null) {
      lock (selfLock) {
        if (cacheImpl.TryGetValue(key, out var node)) {
          var item = node.Value;

          if (!item.Expired) {
            lruImpl.Remove(node);
            lruImpl.AddLast(node);

            return item.Value;
          }
        }
      }

      return null != defaultFunc ? defaultFunc.Invoke() : default;
    }

    private void TryCollect() {
      if (-1 == collectAfter) {
        return;
      }

      if (0 == collectAfter) {
        var collectedCacheImpl = new Dictionary<string, LinkedListNode<Item<T>>>();
        var collectedLruImpl = new LinkedList<Item<T>>();

        foreach (var lruItem in lruImpl.Where(lruItem => !lruItem.Expired)) {
          collectedLruImpl.AddLast(lruItem);
        }

        foreach (var p in cacheImpl.Where(p => !p.Value.Value.Expired)) {
          collectedCacheImpl[p.Key] = p.Value;
        }

        cacheImpl = collectedCacheImpl;
        lruImpl = collectedLruImpl;

        collectAfter = collectFrequency;
      }

      collectAfter = Math.Max(collectAfter - 1, 0);
    }

    private void RemoveFirst() {
      var firstNode = lruImpl.First;

      lruImpl.RemoveFirst();
      cacheImpl.Remove(firstNode.Value.Key);
    }
  }
}