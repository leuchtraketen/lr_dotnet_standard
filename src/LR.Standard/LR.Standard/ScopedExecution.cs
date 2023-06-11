using System;
using System.Collections.Generic;
using System.Text;

namespace LR.Standard;

public sealed class ScopedExecution : IDisposable
{
    private Action _stop;

    public ScopedExecution(Action start = null, Action stop = null)
    {
        _stop = stop;
        start?.Invoke();
    }

    #region IDisposable Support

    private bool disposedValue = false; // Dient zur Erkennung redundanter Aufrufe.

    void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: verwalteten Zustand (verwaltete Objekte) entsorgen.
                _stop?.Invoke();
            }

            // TODO: nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer weiter unten überschreiben.
            // TODO: große Felder auf Null setzen.

            disposedValue = true;
        }
    }

    // TODO: Finalizer nur überschreiben, wenn Dispose(bool disposing) weiter oben Code für die Freigabe nicht verwalteter Ressourcen enthält.
    // ~ScopedExecution()
    // {
    //   // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(bool disposing) weiter oben ein.
    //   Dispose(false);
    // }

    // Dieser Code wird hinzugefügt, um das Dispose-Muster richtig zu implementieren.
    public void Dispose()
    {
        // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(bool disposing) weiter oben ein.
        Dispose(true);
        // TODO: Auskommentierung der folgenden Zeile aufheben, wenn der Finalizer weiter oben überschrieben wird.
        // GC.SuppressFinalize(this);
    }

    #endregion
}
