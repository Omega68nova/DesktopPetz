using System.IO.Pipes;
using System.Text;
using System.Text.Json;

// Synthetic producer for testing the overlay without injecting code into Petz 5.
// The game itself is NOT connected yet.
Console.WriteLine("DesktopPetz mock bridge: Ctrl+C to stop.");
using var shutdown = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; shutdown.Cancel(); };
while (!shutdown.IsCancellationRequested)
{
    try
    {
        await using var pipe = new NamedPipeServerStream(
            "DesktopPetz.State.v1", PipeDirection.Out, 1,
            PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
        Console.WriteLine("Waiting for overlay...");
        await pipe.WaitForConnectionAsync(shutdown.Token);
        Console.WriteLine("Overlay connected.");
        await using var writer = new StreamWriter(pipe, new UTF8Encoding(false)) { AutoFlush = true };
        var origin = System.Windows.Forms.SystemInformation.VirtualScreen;
        var t = 0.0;
        while (pipe.IsConnected && !shutdown.IsCancellationRequested)
        {
            var x = origin.Left + origin.Width / 2.0 + Math.Cos(t) * Math.Min(200, origin.Width / 4);
            var y = origin.Top + origin.Height / 2.0 + Math.Sin(t) * Math.Min(100, origin.Height / 4);
            var frame = new
            {
                version = 1,
                pets = new[] { new { id = "mock-1", name = "Synthetic test pet", x, y, animation = "idle" } }
            };
            await writer.WriteLineAsync(JsonSerializer.Serialize(frame).AsMemory(), shutdown.Token);
            t += 0.025;
            await Task.Delay(33, shutdown.Token);
        }
    }
    catch (OperationCanceledException) { break; }
    catch (IOException) { /* Host disconnected; accept the next client. */ }
    try { await Task.Delay(250, shutdown.Token); }
    catch (OperationCanceledException) { break; }
}
