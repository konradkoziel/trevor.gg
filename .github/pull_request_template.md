## Summary

<!-- What changes for Discord users or for the app, and why. 1–3 sentences. -->

## Changes

<!-- Bullet list grouped by area (command, infrastructure, CI, docs). Name files only where it helps the reviewer. -->

-

## Deploy notes

<!-- Anything to do outside the code: Azure app settings, runtime stack, GitHub secrets, Discord.
     Merging to main deploys to production right away. Write "None" if nothing is needed. -->

None

## Test plan

<!-- Ticked = done before opening the PR. Unticked = still to do after merge (e.g. on Discord). -->

- [ ] `dotnet build -c Release`: 0 warnings
- [ ] `dotnet test`: all green
- [ ] On Discord after deploy:
