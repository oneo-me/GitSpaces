﻿namespace GitSpaces.Commands;

public class Fetch : Command
{
    public Fetch(string repo, string remote, bool prune, Action<string> outputHandler)
    {
        _outputHandler = outputHandler;
        WorkingDirectory = repo;
        Context = repo;
        TraitErrorAsOutput = true;

        var sshKey = new Config(repo).Get($"remote.{remote}.sshkey");
        if (!string.IsNullOrEmpty(sshKey))
        {
            Args = $"-c core.sshCommand=\"ssh -i '{sshKey}'\" ";
        }
        else
        {
            Args = "-c credential.helper=manager ";
        }

        Args += "fetch --progress --verbose ";
        if (prune) Args += "--prune ";
        Args += remote;

        AutoFetch.MarkFetched(repo);
    }

    public Fetch(string repo, string remote, string localBranch, string remoteBranch, Action<string> outputHandler)
    {
        _outputHandler = outputHandler;
        WorkingDirectory = repo;
        Context = repo;
        TraitErrorAsOutput = true;

        var sshKey = new Config(repo).Get($"remote.{remote}.sshkey");
        if (!string.IsNullOrEmpty(sshKey))
        {
            Args = $"-c core.sshCommand=\"ssh -i '{sshKey}'\" ";
        }
        else
        {
            Args = "-c credential.helper=manager ";
        }

        Args += $"fetch --progress --verbose {remote} {remoteBranch}:{localBranch}";
    }

    protected override void OnReadline(string line)
    {
        _outputHandler?.Invoke(line);
    }

    readonly Action<string> _outputHandler;
}

public class AutoFetch
{
    public static bool IsEnabled { get; set; } = false;

    class Job
    {
        public Fetch Cmd;
        public DateTime NextRunTimepoint = DateTime.MinValue;
    }

    static AutoFetch()
    {
        Task.Run(() =>
        {
            while (true)
            {
                if (!IsEnabled)
                {
                    Thread.Sleep(10000);
                    continue;
                }

                var now = DateTime.Now;
                var uptodate = new List<Job>();
                lock (_lock)
                {
                    foreach (var job in _jobs)
                    {
                        if (job.Value.NextRunTimepoint.Subtract(now).TotalSeconds <= 0)
                        {
                            uptodate.Add(job.Value);
                        }
                    }
                }

                foreach (var job in uptodate)
                {
                    job.Cmd.Exec();
                    job.NextRunTimepoint = DateTime.Now.AddSeconds(_fetchInterval);
                }

                Thread.Sleep(2000);
            }
        });
    }

    public static void AddRepository(string repo)
    {
        var job = new Job
        {
            Cmd = new(repo, "--all", true, null)
            {
                RaiseError = false
            },
            NextRunTimepoint = DateTime.Now.AddSeconds(_fetchInterval)
        };

        lock (_lock)
        {
            if (_jobs.ContainsKey(repo))
            {
                _jobs[repo] = job;
            }
            else
            {
                _jobs.Add(repo, job);
            }
        }
    }

    public static void RemoveRepository(string repo)
    {
        lock (_lock)
        {
            _jobs.Remove(repo);
        }
    }

    public static void MarkFetched(string repo)
    {
        lock (_lock)
        {
            if (_jobs.ContainsKey(repo))
            {
                _jobs[repo].NextRunTimepoint = DateTime.Now.AddSeconds(_fetchInterval);
            }
        }
    }

    static readonly Dictionary<string, Job> _jobs = new();
    static readonly object _lock = new();
    static readonly double _fetchInterval = 10 * 60;
}
