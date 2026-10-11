FROM vegardit/openldap:2.6.10
USER root
COPY ldap/schema/fakelegacy.ldif /opt/fake-legacy/schema.ldif
COPY ldap/bootstrap/tree.ldif /opt/ldifs/init_org_tree.ldif
COPY ldap/bootstrap/entries.ldif /opt/ldifs/init_org_entries.ldif
COPY ldap/bootstrap/empty.ldif /opt/ldifs/init_org_ppolicy.ldif
RUN mkdir -p /var/run/slapd \
    && slapadd -n 0 -F /etc/ldap/slapd.d_orig -l /opt/fake-legacy/schema.ldif \
    && slaptest -u -F /etc/ldap/slapd.d_orig
HEALTHCHECK --interval=2s --timeout=3s --start-period=20s --retries=30 \
    CMD ldapwhoami -x -H ldap://127.0.0.1:389 -D "$LDAP_INIT_ROOT_USER_DN" -w "$LDAP_INIT_ROOT_USER_PW" || exit 1
