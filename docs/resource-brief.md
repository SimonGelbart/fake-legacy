# Discovery input: teams

This is a fictional ASP.NET Web Forms resource. Discover its current behavior
from the source and, when available, the running application. Do not change code.

- Resource ID: `teams`
- Application source: `aspnetapp/`
- Entry page: `aspnetapp/Teams.aspx`
- Follow linked pages, registered controls, code-behind and relevant helpers.
- LDAP configuration and synthetic startup inputs: `ldap/` and `compose.yaml`.
- Example application URL: `http://localhost:8080/Teams.aspx`.
- No API or V2 implementation exists yet. Discovery concerns the legacy baseline.

Produce a legacy snapshot, a page/interaction inventory with source evidence,
and focused questions or limitations. Distinguish source facts from runtime
observations and proposals for later behavior. Include related child workflows.

For a blind discovery evaluation, provide only this brief, `aspnetapp/`, `ldap/`
and `compose.yaml`. Keep `evaluation/`, `tests/` and the behavior descriptions in
the README outside the agent's allowed inputs. A prompt restriction alone is not
an access boundary; use a separate input directory if the agent can read the
whole checkout.
