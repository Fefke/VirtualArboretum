namespace VirtualArboretum.Core.Application.DataTransferObjects.InputDTOs;

/// <summary>
/// <b>Mycelium Query Language</b><br/>
/// <b>AND</b>: #has-to-have-hyphae-1 #as-well-as-hyphae-2 #and-hyphae-3 <br/>
/// <b>OR</b>: #either-has-hyphae-1 | #or-hyphae-2 | #or-hyphae-3 <br/>
/// <b>NOT</b>: #has-hyphae-1 !#but-not-hyphae-2<br/>
/// <b>Combination</b>: #has-hyphae-1 !#but--then-not-hyphae-2 | #or-just-hyphae-3<br/>
/// <i>Please note, this defaults to AND logic/ intersection quantity, as long as syntax is correct.</i>
/// </summary>
public record QueryMyceliumInput(
    String HyphaeQuery
    );