`git clone "https://github.com/jharajnish30/gitextensions-rkj" Gitextensions`
Cloning into 'Gitextensions'...
remote: Enumerating objects: 132530, done.
remote: Counting objects: 100% (1359/1359), done.
remote: Compressing objects: 100% (1110/1110), done.
remote: Total 132530 (delta 303), reused 1204 (delta 237), pack-reused 131171
Receiving objects: 100% (132530/132530), 125.29 MiB | 3.63 MiB/s, done.
Resolving deltas: 100% (106021/106021), done.
Updating files: 100% (2541/2541), done.

`git checkout master`
Your branch is up to date with 'origin/master'.
Already on 'master'

`git submodule update --init --recursive`
Submodule path 'Externals/EasyHook': checked out '0296f95158a7da669b44aaa264d8793c478ef648'
Submodule path 'Externals/Git.hub': checked out '30b14eb1eb9509f471c8c098883190d8123f4345'
Submodule path 'Externals/ICSharpCode.TextEditor': checked out '7002c1000582b88bcf38aa121d8f194fb43cd099'
Submodule path 'Externals/conemu-inside': checked out '77af8d6b5080003699eae352db4928b17b42ee08'
Submodule 'Externals/EasyHook' (https://github.com/jharajnish30/EasyHook.git) registered for path 'Externals/EasyHook'
Submodule 'Externals/Git.hub' (https://github.com/jharajnish30/Git.hub.git) registered for path 'Externals/Git.hub'
Submodule 'Externals/ICSharpCode.TextEditor' (https://github.com/jharajnish30/ICSharpCode.TextEditor.git) registered for path 'Externals/ICSharpCode.TextEditor'
Submodule 'Externals/conemu-inside' (https://github.com/jharajnish30/conemu-inside.git) registered for path 'Externals/conemu-inside'
Cloning into 'D:/WORKSPACE/Gitextensions/Externals/EasyHook'...
Cloning into 'D:/WORKSPACE/Gitextensions/Externals/Git.hub'...
Cloning into 'D:/WORKSPACE/Gitextensions/Externals/ICSharpCode.TextEditor'...
Cloning into 'D:/WORKSPACE/Gitextensions/Externals/conemu-inside'...

`git checkout 65f01f39982ccb121c4df7d4b00b506939553958`
Note: switching to '65f01f39982ccb121c4df7d4b00b506939553958'.
M	Externals/Git.hub
M	Externals/ICSharpCode.TextEditor
M	Externals/conemu-inside
You are in 'detached HEAD' state. You can look around, make experimental
changes and commit them, and you can discard any commits you make in this
state without impacting any branches by switching back to a branch.
If you want to create a new branch to retain commits you create, you may
do so (now or later) by using -c with the switch command. Example:
  git switch -c <new-branch-name>
Or undo this operation with:
  git switch -
Turn off this advice by setting config variable advice.detachedHead to false
HEAD is now at 65f01f399 Move the init logic from OnLoad to InternalInitialize (#9591)

`git submodule update --init --recursive`
Submodule path 'Externals/Git.hub': checked out '3ebf19b8fc2ab8a24f54aab64921c88577fff05d'
Submodule path 'Externals/ICSharpCode.TextEditor': checked out '467710117fe6a1df2e96791b51f550a04fccba74'
Submodule path 'Externals/conemu-inside': checked out '6fcd3f5b757e4b49aed8843141f619f5bd38e49d'

`git submodule update --init --recursive -- "Externals/conemu-inside"`
Submodule path 'Externals/conemu-inside': checked out '6fcd3f5b757e4b49aed8843141f619f5bd38e49d'

`4 complementary actions on conemu-insiders module - Main actions follows in next sections`
bb8153c (HEAD -> rkjloc.3.5.4-ext-1) HEAD@{0}: checkout: moving from 6fcd3f5b757e4b49aed8843141f619f5bd38e49d to rkjloc.3.5.4-ext-1
6fcd3f5 HEAD@{1}: checkout: moving from rkjloc.3.5.4-ext-1 to 6fcd3f5b757e4b49aed8843141f619f5bd38e49d
bb8153c (HEAD -> rkjloc.3.5.4-ext-1) HEAD@{2}: commit: config items font, color added/modified
77af8d6 (origin/master, origin/HEAD, master) HEAD@{3}: checkout: moving from 6fcd3f5b757e4b49aed8843141f619f5bd38e49d to rkjloc.3.5.4-ext-1

`git commit --file="D:/WORKSPACE/Gitextensions/.git/COMMITMESSAGE"`
[rkjloc.3.5.4 501f543c1] console, cmd, bash, powershell, git-bash integrated with configurable batch files
 5 files changed, 256 insertions(+), 287 deletions(-)
 rewrite GitUI/CommandsDialogs/SettingsDialog/Pages/ConsoleStyleSettingsPage.Designer.cs (64%)
 create mode 100644 rkj-git-commands.md
`git submodule update --init --recursive -- "Externals/conemu-inside"`
Submodule path 'Externals/conemu-inside': checked out '6fcd3f5b757e4b49aed8843141f619f5bd38e49d'

`git commit --file="D:/WORKSPACE/Gitextensions/.git/COMMITMESSAGE"`
[rkjloc.3.5.4 a9ae0d7e6] Bottom-right panel now collapsible, toolbar icon added to control it
 7 files changed, 67 insertions(+), 1 deletion(-)
 create mode 100644 GitUI/Resources/Icons/LayoutFooter2.png

`git push -u --recurse-submodules=check --progress "origin" refs/heads/rkjloc.rel.3.5:refs/heads/rkjloc.rel.3.5`
Enumerating objects: 47, done.
Counting objects: 100% (47/47), done.
Delta compression using up to 4 threads
Compressing objects: 100% (28/28), done.
Writing objects: 100% (28/28), 5.81 KiB | 991.00 KiB/s, done.
Total 28 (delta 24), reused 0 (delta 0), pack-reused 0
remote: Resolving deltas: 100% (24/24), completed with 19 local objects.
remote:
remote: Create a pull request for 'rkjloc.rel.3.5' on GitHub by visiting:
remote:      https://github.com/jharajnish30/gitextensions/pull/new/rkjloc.rel.3.5
remote:
To https://github.com/jharajnish30/gitextensions
 * [new branch]          rkjloc.rel.3.5 -> rkjloc.rel.3.5
Branch 'rkjloc.rel.3.5' set up to track remote branch 'rkjloc.rel.3.5' from 'origin'.
Done

`git push origin refs/heads/rkjloc.rel.3.5 --verbose`
Enumerating objects: 7, done.
Delta compression using up to 4 threads
Total 4 (delta 3), reused 0 (delta 0), pack-reused 0
POST git-receive-pack (1190 bytes)
Pushing to https://github.com/jharajnish30/gitextensions
To https://github.com/jharajnish30/gitextensions
   a9ae0d7e6..5b200cd7f  rkjloc.rel.3.5 -> rkjloc.rel.3.5
updating local tracking ref 'refs/remotes/origin/rkjloc.rel.3.5'

