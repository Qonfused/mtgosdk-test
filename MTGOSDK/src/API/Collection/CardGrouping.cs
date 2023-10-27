/** @file
  Copyright (c) 2023, Cory Bennett. All rights reserved.
  SPDX-License-Identifier: Apache-2.0
**/

using System;

using MTGOSDK.Core.Reflection;

using WotC.MtGO.Client.Model;
using WotC.MtGO.Client.Model.Collection;
using WotC.MtGO.Client.Model.Core.Collection;


namespace MTGOSDK.API.Collection;

public abstract class CardGrouping<T> : DLRWrapper<ICardGrouping>
{
  /// <summary>
  /// The internal reference for the binding type for the wrapped object.
  /// </summary>
  internal override Type type => typeof(T);

  //
  // ICardGrouping derived properties
  //

  /// <summary>
  /// The unique identifier for this grouping.
  /// </summary>
  public int Id => @base.NetDeckId;

  /// <summary>
  /// The user-defined name for this grouping.
  /// </summary>
  public string Name => @base.Name;

  /// <summary>
  /// The format this grouping is associated with. (e.g. Standard, Historic, etc.)
  /// </summary>
  public IPlayFormat Format => @base.Format;

  /// <summary>
  /// The timestamp of the last modification to this grouping.
  /// </summary>
  public DateTime Timestamp => @base.Timestamp;

  /// <summary>
  /// The total number of cards contained in this grouping.
  /// </summary>
  public int ItemCount => @base.ItemCount;

  /// <summary>
  /// The maximum number of cards that can be contained in this grouping.
  /// </summary>
  public int MaxItems => @base.MaxItems;

  /// <summary>
  /// The hash of the contents of this grouping.
  /// </summary>
  public string Hash => @base.CurrentHash;

  /// <summary>
  /// The items contained in this grouping.
  /// </summary>
  /// <remarks>
  /// Except for the <see cref="Binder"/> and <see cref="Collection"/> classes,
  /// this property will only return items with a quantity greater than zero.
  /// </remarks>
  public IEnumerable<CardQuantityPair> Items =>
    ((IEnumerable<ICardQuantityPair>)
      @base.Items)
        .Where(item =>
          @base.ShouldRemoveZeroQuantityItems == item.Quantity > 0)
        .Select(item => new CardQuantityPair(item));

  /// <summary>
  /// The unique identifiers of the items contained in this grouping.
  /// </summary>
  public IEnumerable<int> ItemIds => @base.ItemIds;

  //
  // ICardGrouping derived methods
  //

  // public virtual bool AddItems(IEnumerable<ICardQuantityPair> items, ulong? operationId = null)
  // public virtual IList<ICardQuantityPair> RemoveItems(IEnumerable<ICardQuantityPair> items, ulong? operationId = null)
}