# Multiplayer
Built on top of [Steam Leaderboards](https://partner.steamgames.com/doc/features/leaderboards)
Weekly seed -> week + year

The goal is to be part of the branch that had gone the farthest.

Every x(10) turns can be turned into a node to be published
Stored as deltas, all the metadata can fit into the leaderboard metadata entry. 
You cannot publish a state that matches a old state (parallel branches) -> do a hash of the state and check it. Stored in metadata?

You can void your node as long as it is not part of the winning branch.
Each node points to the last one, so if a node before you gets voided, your branch is gone too.


Winning branch can claim rewards.

Proposals? Downloadable Metadata? You can only propose for your branch. All proposals are public, you can propose onto someonelses proposed branch.

Check time against [Steamworks Time](https://partner.steamgames.com/doc/api/ISteamUtils)
<https://partner.steamgames.com/doc/api/ISteamUserStats#AttachLeaderboardUGC>
<https://partner.steamgames.com/doc/api/ISteamUserStats#GetDownloadedLeaderboardEntry>
<https://partner.steamgames.com/doc/api/ISteamRemoteStorage#RemoteStorageFileShareResult_>

Metadata can be used to point to other UGC headers and or nodes, leaderboard sorted by best score & branch number?



