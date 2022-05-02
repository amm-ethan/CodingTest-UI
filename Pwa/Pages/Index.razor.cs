using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using Newtonsoft.Json;
using Pwa.Utility;

namespace Pwa.Pages
{
    public partial class Index
    {
        [Inject]
        private IApiRepository? ApiRepository { get; set; }

        [Inject]
        private ISnackbar? Snackbar { get; set; }

        private IBrowserFile? File { get; set; }
        private string? FileName { get; set; }
        private int ProgressValue { get; set; }



        private double percentage = 0;

        private bool isloading;

        private bool disposed;

        private bool haveFile = false;

        public void Dispose()
        {
            disposed = true;
        }

        private void Clear()
        {
            FileName = String.Empty;
            haveFile = false;
        }

        private void OnInputFileChanged(InputFileChangeEventArgs e)
        {
            var files = e.GetMultipleFiles(1);
            File = files[0];
            FileName = files[0].Name;
            ProgressValue = 0;
            haveFile = true;
        }
        private async Task OnUpload()
        {
            isloading = true;
            try
            {
                var content = new MultipartFormDataContent();
                if (File != null)
                {
                    var steam = File.OpenReadStream(1048576);
                    var streamContent = new ProgressiveStreamContent(steam, async (u, p) =>
                    {
                        percentage = p;

                        if (disposed)
                            return;

                        while (ProgressValue < 100)
                        {
                            ProgressValue = Convert.ToInt32(percentage);
                            StateHasChanged();
                            await Task.Delay(500);
                            if (disposed)
                                return;
                        }
                        StateHasChanged();
                    });
                    content.Add(streamContent, "files", FileName!);
                }

                var result = await ApiRepository!.ImportTransactions(content);

                if (result.IsSuccessStatusCode)
                    Snackbar!.Add($"Imported", Severity.Success);
                else
                {
                    if (result.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        var contentText = await result.Content.ReadAsStringAsync();
                        var errorObject = JsonConvert.DeserializeObject<ErrorDetails>(contentText);

                        try
                        {
                            var errorDto = JsonConvert.DeserializeObject<TransactionErrorDto>(errorObject!.Message!.ToString());
                            Snackbar!.Add(errorDto!.Title, Severity.Error);
                            foreach(var error in errorDto.Details!)
                            {
                                Snackbar!.Add($"Error at (row,column) = {error!.Position}", Severity.Error);
                                Snackbar!.Add($"Error header = {error!.Header}", Severity.Error);
                                Snackbar!.Add($"Error value = {error!.Value}", Severity.Error);
                                Snackbar!.Add($"Error detail = {error!.Error}", Severity.Error);
                            }
                        }
                        catch (Exception)
                        {
                            Snackbar!.Add(errorObject!.Message!.ToString(), Severity.Error);
                        }
                    }
                    else
                        Snackbar!.Add($"Unhandled Error Occured", Severity.Error);
                }

                FileName = String.Empty;
                haveFile = false;
            }
            catch (IOException)
            {
                Snackbar!.Add($"File Size Limit for each file is 1MB", Severity.Error);
                FileName = String.Empty;
                haveFile = false;
            }
            finally
            {
                isloading = false;
            }
        }
    }
}
