dotnet ef --startup-project ../EF.MigrationsHelper "$@" --context ProductContext
#TODO-pr ./ef.sh migrations add test
# geeft problemen, same old issue bij product schema migrations
