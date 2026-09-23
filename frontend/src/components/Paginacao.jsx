export default function Paginacao({ resultado, onMudar }) {
  if (resultado.totalPaginas <= 1) return null;

  const { pagina, totalPaginas: ultimaPagina, total } = resultado;

  return (
    <nav className="paginacao" aria-label="Paginação">
      <span className="muted">
        Página {pagina} de {ultimaPagina} · {total} apólices
      </span>
      <div className="paginacao__botoes">
        <button type="button" className="btn btn--small btn--ghost" disabled={pagina <= 1} onClick={() => onMudar(pagina - 1)}>
          Anterior
        </button>
        <button type="button" className="btn btn--small btn--ghost" disabled={pagina >= ultimaPagina} onClick={() => onMudar(pagina + 1)}>
          Próxima
        </button>
      </div>
    </nav>
  );
}
