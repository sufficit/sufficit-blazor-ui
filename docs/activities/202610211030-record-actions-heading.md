# Centralização do cabeçalho de ações

Na validação real do consumidor de anúncios, o cabeçalho Ações tinha alinhamento
start: a regra do SUITable era mais específica que a classe do SUIRecordTable.
Ajustado seletor CSS isolado no componente reutilizável para atravessar o
fragmento da tabela. Nenhum estilo de página. Validado computed textAlign=center no consumidor publicado no tablet.
Build Blazor1.26.1021.1318+56123f25 inclui a alteração, instalado saudável
nos três hosts;59 testes pertinentes do consumidor passaram. Captura real
workspace/tmp/ann-feedback-save-toast.png mostra o cabeçalho centralizado.
