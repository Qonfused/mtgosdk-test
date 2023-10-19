/** @file
  Copyright (c) 2023, Cory Bennett. All rights reserved.
  SPDX-License-Identifier: Apache-2.0
**/

using MTGOSDK.Core;
using MTGOSDK.Core.Reflection;

using WotC.MtGO.Client.Model;
using WotC.MtGO.Client.Model.Core;


namespace MTGOSDK.API.Users;

public sealed class User(dynamic user) : DLRWrapper<IUser>
{
  /// <summary>
  /// Stores an internal reference to the IUser object.
  /// </summary>
  internal override dynamic obj => user;

  public User(int id) : this(UserManager.GetUser(id))
  { }
  public User(string name) : this(UserManager.GetUser(name))
  { }
  public User(int id, string name) : this(UserManager.GetUser(id, name))
  { }

  //
  // IUser wrapper properties
  //

  /// <summary>
  /// The Login ID of the user.
  /// </summary>
  public int Id => @base.Id;

  /// <summary>
  /// The display name of the user.
  /// </summary>
  public string Name => @base.Name;

  /// <summary>
  /// The Catalog ID of the user's avatar.
  /// </summary>
  public int AvatarId => @base.AvatarID;

  /// <summary>
  /// The user's avatar resource.
  /// </summary>
  public IAvatar Avatar => Proxy<IAvatar>.As(@base.CurrentAvatar);

  /// <summary>
  /// Whether the account is not a fully activated account.
  /// </summary>
  public bool IsGuest => @base.IsGuest;

  /// <summary>
  /// Whether the user is added as a buddy of the current user.
  /// </summary>
  public bool IsBuddy => @base.IsBuddy;

  /// <summary>
  /// Whether the user is blocked by the current user.
  /// </summary>
  public bool IsBlocked => @base.IsBlocked;

  /// <summary>
  /// Whether the user is logged in and visible to other users.
  /// </summary>
  public bool IsLoggedIn => @base.IsLoggedInAndVisible;

  /// <summary>
  /// The user's last login timestamp.
  /// </summary>
  public string LastLogin => @base.LastLogin;
}