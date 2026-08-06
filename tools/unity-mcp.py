#!/usr/bin/env python3
"""Launch the MCP for Unity server, routed to THIS checkout's Unity Editor.

Invoked from .mcp.json. The Unity subfolder name is passed as argv[1] so this
script stays identical across repos/worktrees.

It derives the Unity project hash like the Editor bridge
(ProjectIdentityUtility.ComputeProjectHash: hex of sha1(Application.dataPath)),
then starts the server with --default-instance <hash>. Routing by hash (which is
a function of the absolute project path) is what makes a worktree resolve to its
own Editor instead of the main checkout's - their project names are identical, only
the path-derived hash differs.

We use the first 8 hex chars: that is exactly what Unity names its stdio status
file (unity-mcp-status-<8>.json), so it matches by `==` in stdio discovery, and it
also prefix-matches the 16-char hash the HTTP hub reports - one value works in both.
"""
import hashlib
import os
import subprocess
import sys

SERVER_PACKAGE = "mcpforunityserver==10.1.0"

unity_subdir = sys.argv[1]

# CLAUDE_PROJECT_DIR is set by Claude Code to the checkout/worktree root. Forward
# slashes + no trailing slash match Unity's Application.dataPath on every platform.
root = (os.environ.get("CLAUDE_PROJECT_DIR") or os.getcwd()).replace("\\", "/").rstrip("/")
data_path = f"{root}/{unity_subdir}/Assets"
project_hash = hashlib.sha1(data_path.encode("utf-8")).hexdigest()[:8]

sys.exit(subprocess.run([
    "uvx", "--from", SERVER_PACKAGE, "mcp-for-unity",
    "--transport", "stdio",
    "--default-instance", project_hash,
]).returncode)
