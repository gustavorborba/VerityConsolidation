namespace ConsolidationApi.Endpoints
{
    public static class ConsolidationEndpoints
    {
        public static void MapConsolidationEndpoints(this IEndpointRouteBuilder routes)
        {
            routes.MapPost("consolidation/run-by-range", async (DateTime startDate, DateTime endDate, IConsolidationService service) =>
            {
                //Aqui quem chamou irá aguardar uma resposta, isso foi feito apenas para facilitar o teste e visualização através da confirmação.
                //Em um cenário real onde há um endpoint para forçar execução em uma data especifica.
                //O processo de consolidação nesse caso seria feito em background, e o retorno apenas um Ok.
                await service.ConsolidationProcess(startDate, endDate);

                Results.Ok("Process Completed");
            })
            .WithName("Run By Date Range")
            .RequireAuthorization();

            routes.MapGet("consolidation/export", async (int consolidationsNumber, IConsolidationService service) =>
            {
                var fileBytes = await service.GenerateExcelReport(consolidationsNumber);
                return Results.File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "consolidate.xlsx");
            })
            .WithName("Get Consolidation File")
            .RequireAuthorization();

            routes.MapGet("consolidation/all", async (IConsolidationService service) =>
            {
                var result = await service.GetAll();
                return Results.Ok(result);
            })
            .WithName("Get All Consolidations")
            .RequireAuthorization();
        }
    }
}
