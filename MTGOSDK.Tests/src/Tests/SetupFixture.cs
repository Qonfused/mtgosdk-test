/** @file
  Copyright (c) 2024, Cory Bennett. All rights reserved.
  SPDX-License-Identifier: Apache-2.0
**/

using System.IO;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using MTGOSDK.API;
using MTGOSDK.API.Play.History;
using MTGOSDK.Core.Reflection;
using MTGOSDK.Core.Remoting;
using MTGOSDK.Core.Security;

using MTGOSDK.NUnit.Logging;


namespace MTGOSDK.Tests;

[SetUpFixture]
public class SetupFixture : DLRWrapper<Client>
{
  /// <summary>
  /// A shared setup fixture that can be used to interact with the global state
  /// of the test runner.
  /// </summary>
  public class Shared : SetupFixture
  {
#pragma warning disable CS1998
    public override async Task RunBeforeAnyTests() { }
    public override async Task RunAfterAnyTests() { }
#pragma warning restore CS1998
  }

  /// <summary>
  /// The default client instance to interact with the MTGO API.
  /// </summary>
  /// <remarks>
  /// This is a shared instance that is intended to be interacted with by all
  /// tests in the test suite in a multi-threaded environment to avoid redundant
  /// setup and teardown operations with the <see cref="SetupFixture"/> class.
  /// </remarks>
  public static Client client { get; private set; } = null!;

  [OneTimeSetUp, CancelAfter(/* 5 min */ 300_000)]
  public virtual async Task RunBeforeAnyTests()
  {
    // Skip if the client has already been initialized.
    if (Client.HasStarted && client != null) return;

    client = new Client(
      new ClientOptions
      {
        CreateProcess = true,
        StartMinimized = true,
        // DestroyOnExit = true,
        AcceptEULAPrompt = true
      },
      loggerProvider: new NUnitLoggerProvider(LogLevel.Trace)
    );

    // Ensure the MTGO client is not interactive (with an existing user session).
    Assert.That(Client.IsInteractive, Is.False);

    if (!Client.IsConnected)
    {
      DotEnv.LoadFile();
      // Waits until the client has loaded and is ready.
      await client.LogOn(
        username: DotEnv.Get("USERNAME"), // String value
        password: DotEnv.Get("PASSWORD")  // SecureString value
      );
      Assert.That(Client.IsLoggedIn, Is.True);

      // Revalidate the client's reported interactive state.
      Assert.That(Client.IsInteractive, Is.False);
    }

    // Restore any game history previously saved to the file system.
    if (Try(() => DotEnv.Get("GAME_HISTORY_FILE")) is string gameHistoryFile)
    {
      // Load the game history file if it exists.
      if (File.Exists(gameHistoryFile))
        HistoryManager.MergeGameHistory(gameHistoryFile);
    }

    client.ClearCaches();
  }

  [OneTimeTearDown, CancelAfter(/* 10 seconds */ 10_000)]
  public virtual async Task RunAfterAnyTests()
  {
    // Skip if the client has already been disposed.
    if (!RemoteClient.IsInitialized && client == null) return;

    // Log off the client to ensure that the user session terminates.
    await client.LogOff();
    Assert.That(Client.IsLoggedIn, Is.False);

    // Set a callback to indicate when the client has been disposed.
    bool isDisposed = false;
    RemoteClient.Disposed += (s, e) => isDisposed = true;

    // Safely dispose of the client instance.
    client.Dispose();
    client = null!;
    if (!await WaitUntil(() => isDisposed)) // Waits at most 5 seconds.
    {
      Assert.Fail("The client was not disposed within the timeout period.");
    }

    // Verify that all remote handles have been reset.
    Assert.That(RemoteClient.IsInitialized, Is.False);
    Assert.That(RemoteClient.Port, Is.Null);
  }
}
