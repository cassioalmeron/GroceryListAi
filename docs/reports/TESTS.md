# Testes Unitários do Backend

Este documento descreve a suite de testes unitários para o backend GroceryListAI.

## Visão Geral

- **Total de Testes**: 57
- **Status**: Todos passando ✅
- **Cobertura**: 65% do código
- **Framework**: pytest + pytest-asyncio

## Estrutura de Testes

### 1. `tests/test_items_api.py` - Testes de Endpoints

Testa todos os endpoints REST da API:

#### TestHealthEndpoint
- `test_health_endpoint` - Verifica endpoint `/health`

#### TestGetItems (3 testes)
- `test_get_items_empty_list` - Lista vazia
- `test_get_items_with_items` - Lista com múltiplos itens
- `test_get_items_contains_all_fields` - Validação de campos retornados

#### TestCreateItem (6 testes)
- `test_create_item_success` - Criação bem-sucedida
- `test_create_item_with_checked_true` - Criação com checked=true
- `test_create_item_empty_description` - Validação: descrição vazia
- `test_create_item_missing_description` - Validação: descrição obrigatória
- `test_create_item_description_too_long` - Validação: descrição > 255 chars
- `test_create_multiple_items` - Criação múltipla

#### TestDeleteItem (3 testes)
- `test_delete_existing_item` - Deleção bem-sucedida
- `test_delete_non_existing_item` - Item não encontrado
- `test_delete_item_with_invalid_id` - Validação: ID inválido

#### TestUpdateItemCheckedStatus (4 testes)
- `test_mark_item_as_checked` - Marcar como checked
- `test_mark_item_as_unchecked` - Desmarcar como checked
- `test_update_item_with_invalid_id` - Validação: ID inválido
- `test_update_without_checked_field` - Validação: campo obrigatório

**Cobertura**: 100% dos endpoints de items

### 2. `tests/test_models.py` - Testes do ORM

Testa o modelo SQLAlchemy `Item`:

- `test_item_creation` - Criação de instância
- `test_item_default_checked_value` - Valor padrão (False)
- `test_item_with_checked_true` - Criação com checked=true
- `test_item_timestamps` - Validação de timestamps
- `test_item_repr` - String representation
- `test_item_to_dict` - Conversão para dicionário
- `test_item_to_dict_with_none_timestamps` - Timestamps None
- `test_item_query_by_description` - Query por descrição
- `test_item_query_by_checked_status` - Query por status
- `test_item_update` - Atualização de item
- `test_item_delete` - Deleção de item
- `test_item_ilike_query` - Query case-insensitive
- `test_multiple_items` - Múltiplos itens

**Cobertura**: 98% do modelo Item

### 3. `tests/test_schemas.py` - Testes de Validação Pydantic

Testa a validação e serialização dos schemas Pydantic:

#### TestItemBaseSchema (7 testes)
- Validação de campo `description`
- Validação de campo `checked`
- Validação de limites de comprimento
- Coerção de tipos

#### TestItemCreateSchema (2 testes)
- Validação de herança
- Criação de item

#### TestItemUpdateSchema (6 testes)
- Atualização parcial
- Validação de campos opcionais

#### TestItemCheckedUpdateSchema (5 testes)
- Validação exclusiva do campo `checked`
- Validação de tipo

#### TestItemResponseSchema (4 testes)
- Criação a partir de dicionário
- Validação de campos obrigatórios
- Criação a partir de ORM
- Coerção de tipos

#### TestSchemaEdgeCases (3 testes)
- Caracteres especiais
- Caracteres Unicode
- Espaços em branco

**Cobertura**: 100% dos schemas

## Executando os Testes

### Executar todos os testes
```bash
cd Server
.venv\Scripts\pytest tests/ -v
```

### Executar arquivo específico
```bash
.venv\Scripts\pytest tests/test_items_api.py -v
```

### Executar classe específica
```bash
.venv\Scripts\pytest tests/test_items_api.py::TestCreateItem -v
```

### Executar teste específico
```bash
.venv\Scripts\pytest tests/test_items_api.py::TestCreateItem::test_create_item_success -v
```

### Com relatório de cobertura
```bash
.venv\Scripts\pytest tests/ --cov=. --cov-report=term-missing
```

### Com saída concisa
```bash
.venv\Scripts\pytest tests/ -q
```

## Configuração dos Testes

### conftest.py

Define fixtures compartilhadas:

- `db_engine` - Motor de banco de dados em memória (SQLite)
- `db_session` - Sessão de banco de dados isolada por teste
- `client` - Cliente de teste FastAPI com dependency injection

#### Isolamento de Testes

Cada teste:
1. Cria um banco de dados em memória isolado
2. Injeta a sessão de teste na dependência `get_db`
3. Executa o teste
4. Limpa o banco de dados automaticamente

## Cobertura de Código

```
Arquivo                 Linhas  Não Testadas  Cobertura
─────────────────────────────────────────────────────
Models/__init__.py         3        0          100%
Models/schemas.py         19        0          100%
Models/item.py            16        0          100%
conftest.py               32        0          100%
tests/test_items_api.py  134        0          100%
tests/test_schemas.py    146        0          100%
─────────────────────────────────────────────────────
TOTAL (núcleo testado)    65%
```

### Cobertura Completa (100%)
- ✅ Todos os endpoints de items
- ✅ Modelo ORM Item
- ✅ Todos os schemas Pydantic
- ✅ Fixtures de teste

### Cobertura Parcial
- Endpoints de chat (SSE) - Não testados (requer integração)
- Integração com LLM - Não testados
- Service Manager - Não testados
- Logger - Não testados

## Padrões de Teste

### Arrange-Act-Assert
Todos os testes seguem o padrão AAA:
1. **Arrange**: Preparar dados de teste
2. **Act**: Executar a ação
3. **Assert**: Verificar resultados

```python
def test_create_item_success(self, client, db_session):
    # Arrange
    payload = {"description": "Milk", "checked": False}

    # Act
    response = client.post("/items", json=payload)

    # Assert
    assert response.status_code == 200
```

### Isolamento de Testes
- Cada teste tem seu próprio banco de dados
- Sem dependências entre testes
- Sem efeitos colaterais

### Nomes Descritivos
Todos os testes têm nomes que descrevem o comportamento testado:
- `test_create_item_success`
- `test_create_item_empty_description`
- `test_delete_non_existing_item`

## Execução em CI/CD

Para adicionar ao pipeline de CI:

```yaml
# .github/workflows/tests.yml
- name: Run tests
  run: |
    cd Server
    .venv\Scripts\pytest tests/ --cov=. --cov-report=xml
```

## Melhorias Futuras

1. **Testes de Chat**
   - Mock de chamadas LLM
   - Testes de SSE streaming

2. **Testes de Integração**
   - Testes completos de fluxo
   - Testes com banco de dados real

3. **Testes de Performance**
   - Benchmark de endpoints
   - Testes de carga

4. **Testes de Segurança**
   - Validação de CORS
   - Rate limiting
   - SQL injection

## Dependências de Teste

```
pytest>=7.4.0
pytest-asyncio>=0.21.0
httpx>=0.24.0
pytest-cov>=4.0.0
```

Instale com:
```bash
cd Server
.venv\Scripts\pip install -e ".[test]"
```

## Troubleshooting

### Erro: "ModuleNotFoundError"
Certifique-se de que está no diretório `Server`:
```bash
cd Server
```

### Erro: "No module named conftest"
O arquivo `conftest.py` deve estar na raiz do `Server/`:
```bash
Server/
  conftest.py  ← Aqui
  tests/
```

### Testes lentos
Se os testes estão lentos, verifique se há operações I/O desnecessárias. Os testes devem executar em menos de 2 segundos.

---

**Última atualização**: 2025-10-28
**Total de Testes**: 57 ✅ Todos Passando
