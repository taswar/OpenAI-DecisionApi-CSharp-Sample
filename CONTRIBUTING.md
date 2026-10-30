# Contributing

Thanks for helping improve this sample. It's a small .NET 10 console app that shows how to call the Azure OpenAI Decisions API from C#, using predicate, choice and score questions. Contributions that make it clearer, more correct, or more useful to other .NET developers are welcome.

## Ways to contribute

- **Report a bug.** Something fails, or the README doesn't match what the code does.
- **Fix the docs.** Typos, unclear steps, missing troubleshooting cases.
- **Improve the sample.** For example, an image input example (the Decisions API can judge images as well as text), more question types in one request, or a clearer way to handle low-confidence answers.
- **Share results.** If you ran the sample against your own tickets or data, an issue describing what you saw helps others choose their thresholds.

For anything larger than a small fix, open an issue first so we can agree on the approach before you write code.

## Set up

1. Install the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).
2. Clone the repo and build it:

   ```bash
   git clone https://github.com/taswar/OpenAI-DecisionApi-CSharp-Sample.git
   cd OpenAI-DecisionApi-CSharp-Sample
   dotnet build
   ```

3. To run it, create an Azure OpenAI resource with a `gpt-6-luna` deployment, then set `AZURE_OPENAI_ENDPOINT` (and optionally `AZURE_OPENAI_DEPLOYMENT` and `AZURE_OPENAI_API_KEY`) with user secrets, as described in the [README](README.md#configuration). Use an API key, or sign in with `az login` for Entra ID.

## Guidelines

- **Never commit secrets.** No API keys, endpoints, tenant IDs or tokens in code, config files or screenshots. Settings belong in user secrets or environment variables.
- **Keep it a sample.** The code should stay short and readable from top to bottom. Prefer clarity over abstraction, and add a dependency only when it clearly helps.
- **Match the existing style.** Target `net10.0`, keep nullable reference types on, and follow the naming and layout already in `Program.cs`.
- **Handle every answer type.** If you add a question, make sure `PrintAnswer` still handles a `refusal` for it. A refusal is not a low probability.
- **Keep the README in sync.** If you change behaviour, settings or output, update the README in the same pull request, including the example output.
- **One change per pull request.** Small, focused pull requests are easier to review and merge.

## Pull request checklist

- [ ] `dotnet build` succeeds with no new warnings.
- [ ] The app runs against your own Azure OpenAI deployment, or you explain why you couldn't run it.
- [ ] No secrets, endpoints or personal data are in the diff.
- [ ] The README reflects any change in behaviour, settings or output.
- [ ] The pull request description says what changed and why.

The build workflow runs on every pull request and has to pass before a merge.

## Reporting a bug

Open an issue and include:

- What you ran and what you expected.
- The full error message, with any keys or endpoints removed.
- Your .NET SDK version (`dotnet --version`) and operating system.
- Whether you used an API key or Entra ID. For Entra ID, check that your account has the **Cognitive Services OpenAI User** role on the resource.
- Your deployment name, if it isn't `gpt-6-luna`.

## Security issues

Please don't report security problems in a public issue. Use **Security → Report a vulnerability** on the repo to send a private report.

## License

By contributing, you agree that your contributions are licensed under the [MIT License](LICENSE) that covers this project.
