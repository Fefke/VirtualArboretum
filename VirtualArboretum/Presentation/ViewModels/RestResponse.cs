namespace VirtualArboretum.Presentation.ViewModels;

/// <summary>
/// Stellt eine standardisierte REST-API-Antwort dar.
/// </summary>
public class RestResponse<T>
{
    /// <summary>
    /// Status der Anfrage ("Success" oder "Error").
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// Optionale Nachricht, die den Status oder Fehler erklärt.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Optionale Daten, die bei erfolgreicher Anfrage zurückgegeben werden.
    /// </summary>
    public T Data { get; set; }

    /// <summary>
    /// Erstellt eine Erfolgsmeldung mit Daten.
    /// </summary>
    public static RestResponse<T> Success(T data, string message = null)
    {
        return new RestResponse<T> { Status = "Success", Data = data, Message = message };
    }

    /// <summary>
    /// Erstellt eine Erfolgsmeldung ohne Daten, nur mit einer Nachricht.
    /// </summary>
    public static RestResponse<T> SuccessMessage(string message)
    {
        return new RestResponse<T> { Status = "Success", Message = message };
    }

    /// <summary>
    /// Erstellt eine Fehlermeldung.
    /// </summary>
    public static RestResponse<T> Error(string message)
    {
        return new RestResponse<T> { Status = "Error", Message = message };
    }
}

/// <summary>
/// Vereinfachte RestResponse-Klasse für Antworten ohne Daten.
/// </summary>
public class RestResponse : RestResponse<object>
{
    /// <summary>
    /// Erstellt eine Erfolgsmeldung mit einer Nachricht.
    /// </summary>
    public new static RestResponse SuccessMessage(string message)
    {
        return new RestResponse { Status = "Success", Message = message };
    }

    /// <summary>
    /// Erstellt eine Fehlermeldung.
    /// </summary>
    public new static RestResponse Error(string message)
    {
        return new RestResponse { Status = "Error", Message = message };
    }
}
